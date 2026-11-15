using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using NotificationService.Api.Middleware;

namespace NotificationService.Api.Tests;

public sealed class GlobalExceptionMiddlewareTests
{
    private const string SensitiveMessage = "Password=SuperSecret; internal stack detail";

    [Fact]
    public async Task UnhandledException_ReturnsProblemDetailsWithoutInternalDetails()
    {
        var logger = new CollectingLogger();
        var context = CreateContext();
        var middleware = new GlobalExceptionMiddleware(_ => throw new InvalidOperationException(SensitiveMessage), logger);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith(GlobalExceptionMiddleware.ProblemJsonContentType, context.Response.ContentType, StringComparison.Ordinal);

        var body = await ReadBodyAsync(context);
        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal(500, root.GetProperty("status").GetInt32());
        Assert.Equal(GlobalExceptionMiddleware.InternalErrorCode, root.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("traceId").GetString()));
        Assert.Equal("/api/v1/test", root.GetProperty("instance").GetString());
        Assert.DoesNotContain("SuperSecret", body, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), body, StringComparison.Ordinal);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.IsType<InvalidOperationException>(entry.Exception);
        Assert.DoesNotContain("SuperSecret", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnhandledException_UsesTraceIdentifierWhenNoActivityExists()
    {
        var context = CreateContext();
        context.TraceIdentifier = "trace-123";
        var middleware = new GlobalExceptionMiddleware(_ => throw new InvalidOperationException(), new CollectingLogger());

        await middleware.InvokeAsync(context);

        using var json = JsonDocument.Parse(await ReadBodyAsync(context));
        var traceId = json.RootElement.GetProperty("traceId").GetString();
        Assert.Equal(System.Diagnostics.Activity.Current?.Id ?? "trace-123", traceId);
    }

    [Fact]
    public async Task ResponseAlreadyStarted_RethrowsWithoutWriting()
    {
        var context = CreateContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());
        var middleware = new GlobalExceptionMiddleware(_ => throw new InvalidOperationException(), new CollectingLogger());

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
    }

    [Fact]
    public async Task ClientCancellation_IsNotConvertedIntoServerError()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var context = CreateContext();
        context.RequestAborted = cancellation.Token;
        var logger = new CollectingLogger();
        var middleware = new GlobalExceptionMiddleware(_ => throw new OperationCanceledException(cancellation.Token), logger);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Empty(await ReadBodyAsync(context));
        Assert.DoesNotContain(logger.Entries, entry => entry.Level >= LogLevel.Error);
    }

    [Fact]
    public async Task CancellationNotRequestedByClient_IsTreatedAsServerError()
    {
        var context = CreateContext();
        var middleware = new GlobalExceptionMiddleware(_ => throw new TaskCanceledException(), new CollectingLogger());

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task SuccessfulRequest_PassesThroughUnchanged()
    {
        var context = CreateContext();
        var middleware = new GlobalExceptionMiddleware(
            ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                return Task.CompletedTask;
            },
            new CollectingLogger());

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        Assert.Empty(await ReadBodyAsync(context));
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private sealed class StartedResponseFeature : HttpResponseFeature
    {
        public override bool HasStarted => true;
    }

    private sealed class CollectingLogger : ILogger<GlobalExceptionMiddleware>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception), exception));
        }
    }
}
