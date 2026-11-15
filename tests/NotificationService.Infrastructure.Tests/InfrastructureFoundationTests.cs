using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NotificationService.Application.Abstractions.Time;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Time;

namespace NotificationService.Infrastructure.Tests;

/// <summary>
/// Pruebas de fundación: no abren conexiones ni dependen de servicios externos.
/// </summary>
public sealed class InfrastructureFoundationTests
{
    private const string ConnectionStringKey = "ConnectionStrings:DefaultConnection";

    private const string ValidConnectionString =
        "Host=localhost;Port=5432;Database=notification_test;Username=test;Password=placeholder";

    [Fact]
    public void AddInfrastructure_WithValidConnectionString_BuildsContainerAndBindsOptions()
    {
        using var provider = BuildProvider((ConnectionStringKey, ValidConnectionString));

        var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        Assert.Equal(ValidConnectionString, options.ConnectionString);
    }

    [Fact]
    public void AddInfrastructure_RegistersOptionsValidationOnStart()
    {
        using var provider = BuildProvider();

        var startupValidator = provider.GetRequiredService<IStartupValidator>();

        Assert.Throws<OptionsValidationException>(startupValidator.Validate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("this is not a connection string")]
    [InlineData("UnknownKeyword=value;Host=localhost;Database=db")]
    [InlineData("Port=5432;Database=notification_test")]
    [InlineData("Host=localhost;Port=5432")]
    public void DatabaseOptions_WithMissingOrInvalidConnectionString_FailValidation(string? connectionString)
    {
        using var provider = BuildProvider((ConnectionStringKey, connectionString));

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<DatabaseOptions>>().Value);
    }

    [Fact]
    public void DatabaseOptions_LegacyDatabaseSection_IsNotHonored()
    {
        using var provider = BuildProvider(("Database:ConnectionString", ValidConnectionString));

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<DatabaseOptions>>().Value);
    }

    [Fact]
    public void DatabaseOptions_ValidationErrors_DoNotExposeConnectionStringValues()
    {
        const string secret = "TopSecretPasswordValue";
        using var provider = BuildProvider(
            (ConnectionStringKey, "Port=5432;Database=notification_test;Password=" + secret));

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<DatabaseOptions>>().Value);

        Assert.DoesNotContain(secret, exception.Message, StringComparison.Ordinal);
        Assert.All(exception.Failures, failure => Assert.DoesNotContain(secret, failure, StringComparison.Ordinal));
    }

    [Fact]
    public void AddInfrastructure_DbContextUsesNpgsqlWithoutOpeningAConnection()
    {
        using var provider = BuildProvider((ConnectionStringKey, ValidConnectionString));
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", dbContext.Database.ProviderName);
    }

    [Fact]
    public void AddInfrastructure_RegistersSystemClockAsSingleton()
    {
        using var provider = BuildProvider((ConnectionStringKey, ValidConnectionString));

        var first = provider.GetRequiredService<IClock>();
        var second = provider.GetRequiredService<IClock>();

        Assert.IsType<SystemClock>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void SystemClock_ReturnsUtcInstant()
    {
        var now = new SystemClock().UtcNow;

        Assert.Equal(TimeSpan.Zero, now.Offset);
    }

    [Fact]
    public void AddInfrastructure_DoesNotRegisterUnverifiedDependencyHealthChecks()
    {
        using var provider = BuildProvider((ConnectionStringKey, ValidConnectionString));

        var healthCheckOptions = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

        Assert.NotNull(provider.GetService<HealthCheckService>());
        Assert.Empty(healthCheckOptions.Registrations);
    }

    private static ServiceProvider BuildProvider(params (string Key, string? Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(setting => new KeyValuePair<string, string?>(setting.Key, setting.Value)))
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
