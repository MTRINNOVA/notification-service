using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using NotificationService.Worker.Common;

namespace NotificationService.Worker.Tests;

public sealed class WorkerHostCompositionTests
{
    private const string ValidConnectionString =
        "Host=localhost;Port=5432;Database=notification_test;Username=test;Password=placeholder";

    [Fact]
    public void WorkerAssemblyMarker_ShouldExist()
    {
        Assert.NotNull(typeof(WorkerAssemblyMarker));
    }

    [Fact]
    public void AddWorkerHost_RegistersWorkerHostedService()
    {
        using var host = BuildHost(ValidConnectionString);

        var hostedServices = host.Services.GetServices<IHostedService>();

        Assert.Single(hostedServices.OfType<WorkerService>());
    }

    [Fact]
    public void AddWorkerHost_WithValidConfiguration_PassesStartupValidation()
    {
        using var host = BuildHost(ValidConnectionString);

        host.Services.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void AddWorkerHost_WithoutConnectionString_FailsStartupValidation()
    {
        using var host = BuildHost(connectionString: null);

        Assert.Throws<OptionsValidationException>(host.Services.GetRequiredService<IStartupValidator>().Validate);
    }

    [Fact]
    public void AddWorkerHost_UsesStructuredJsonConsoleLogging()
    {
        using var host = BuildHost(ValidConnectionString);

        var providers = host.Services.GetServices<ILoggerProvider>().ToList();
        var consoleOptions = host.Services.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue;
        var jsonOptions = host.Services.GetRequiredService<IOptionsMonitor<JsonConsoleFormatterOptions>>().CurrentValue;

        Assert.NotEmpty(providers);
        Assert.All(providers, provider => Assert.IsType<ConsoleLoggerProvider>(provider));
        Assert.Equal(ConsoleFormatterNames.Json, consoleOptions.FormatterName);
        Assert.True(jsonOptions.UseUtcTimestamp);
        Assert.True(jsonOptions.IncludeScopes);
    }

    private static IHost BuildHost(string? connectionString)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true,
            EnvironmentName = Environments.Production
        });

        if (connectionString is not null)
        {
            builder.Configuration.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", connectionString)
            ]);
        }

        return builder.AddWorkerHost().Build();
    }
}
