using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using NotificationService.Scheduler;
using NotificationService.Scheduler.Common;

namespace NotificationService.Architecture.Tests;

/// <summary>
/// Composición del host Scheduler. Se ubica aquí porque no existe NotificationService.Scheduler.Tests;
/// crear ese proyecto es una decisión pendiente (ver docs/operations/technical-pending-register.md).
/// </summary>
public sealed class SchedulerHostCompositionTests
{
    private const string ValidConnectionString =
        "Host=localhost;Port=5432;Database=notification_test;Username=test;Password=placeholder";

    [Fact]
    public void AddSchedulerHost_RegistersSchedulerHostedService()
    {
        using var host = BuildHost(ValidConnectionString);

        Assert.Single(host.Services.GetServices<IHostedService>().OfType<SchedulerService>());
    }

    [Fact]
    public void AddSchedulerHost_WithValidConfiguration_PassesStartupValidation()
    {
        using var host = BuildHost(ValidConnectionString);

        host.Services.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void AddSchedulerHost_WithoutConnectionString_FailsStartupValidation()
    {
        using var host = BuildHost(connectionString: null);

        Assert.Throws<OptionsValidationException>(host.Services.GetRequiredService<IStartupValidator>().Validate);
    }

    [Fact]
    public void AddSchedulerHost_UsesStructuredJsonConsoleLogging()
    {
        using var host = BuildHost(ValidConnectionString);

        var consoleOptions = host.Services.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue;
        var jsonOptions = host.Services.GetRequiredService<IOptionsMonitor<JsonConsoleFormatterOptions>>().CurrentValue;

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

        return builder.AddSchedulerHost().Build();
    }
}
