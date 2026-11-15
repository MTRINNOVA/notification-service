using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure;

namespace NotificationService.Infrastructure.Tests;

public sealed class InfrastructureFoundationTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterHealthChecks()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("Database:ConnectionString", "Host=localhost;Port=5432;Database=test;Username=test;Password=test")
            ])
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var healthChecks = provider.GetService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();

        Assert.NotNull(healthChecks);
    }
}
