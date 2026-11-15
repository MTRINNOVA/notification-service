using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using NotificationService.Api.Common;

namespace NotificationService.Api.Tests;

/// <summary>
/// Pruebas de fundación del host API. No requieren PostgreSQL: ningún endpoint actual abre conexiones.
/// </summary>
public sealed class ApiFoundationTests
{
    [Fact]
    public void ApiAssemblyMarker_ShouldExist()
    {
        Assert.NotNull(typeof(ApiAssemblyMarker));
    }

    [Fact]
    public async Task HealthLive_ReturnsHealthy()
    {
        await using var factory = new ApiTestFactory(Environments.Production);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task HealthReady_IsExposedSeparatelyFromLiveness()
    {
        // Todavía no hay dependencias con comprobación real; se añadirán con la etiqueta "ready" en su fase.
        await using var factory = new ApiTestFactory(Environments.Production);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_InDevelopment_ServesOpenApi31DocumentAndSwaggerUi()
    {
        await using var factory = new ApiTestFactory(Environments.Development);
        using var client = factory.CreateClient();

        var document = await client.GetAsync("/openapi/v1.json");
        var swaggerUi = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, document.StatusCode);
        Assert.Contains("\"openapi\": \"3.1", await document.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, swaggerUi.StatusCode);
    }

    [Fact]
    public async Task OpenApi_InProduction_IsNotExposedByDefault()
    {
        await using var factory = new ApiTestFactory(Environments.Production);
        using var client = factory.CreateClient();

        var document = await client.GetAsync("/openapi/v1.json");
        var swaggerUi = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.NotFound, document.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, swaggerUi.StatusCode);
    }

    [Fact]
    public async Task OpenApi_InProduction_CanBeEnabledExplicitly()
    {
        await using var factory = new ApiTestFactory(
            Environments.Production,
            new Dictionary<string, string?> { ["OpenApi:Enabled"] = "true" });
        using var client = factory.CreateClient();

        var document = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, document.StatusCode);
    }

    [Fact]
    public void Startup_WithoutConnectionString_FailsOptionsValidation()
    {
        using var factory = new ApiTestFactory(
            Environments.Production,
            new Dictionary<string, string?> { [ApiTestFactory.ConnectionStringKey] = string.Empty });

        var exception = Record.Exception(() => factory.CreateClient());

        Assert.NotNull(exception);
        Assert.Contains(Flatten(exception), inner => inner is OptionsValidationException);
    }

    [Fact]
    public async Task Logging_UsesStructuredJsonConsoleOnly()
    {
        await using var factory = new ApiTestFactory(Environments.Production);
        _ = factory.CreateClient();
        var services = factory.Services;

        var providers = services.GetServices<ILoggerProvider>().ToList();
        var consoleOptions = services.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue;
        var jsonOptions = services.GetRequiredService<IOptionsMonitor<JsonConsoleFormatterOptions>>().CurrentValue;

        Assert.NotEmpty(providers);
        Assert.All(providers, provider => Assert.IsType<ConsoleLoggerProvider>(provider));
        Assert.Equal(ConsoleFormatterNames.Json, consoleOptions.FormatterName);
        Assert.True(jsonOptions.UseUtcTimestamp);
        Assert.True(jsonOptions.IncludeScopes);
    }

    private static IEnumerable<Exception> Flatten(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            yield return current;

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions.SelectMany(Flatten))
                {
                    yield return inner;
                }
            }
        }
    }
}
