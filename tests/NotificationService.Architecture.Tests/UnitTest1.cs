using NetArchTest.Rules;
using NotificationService.Api.Common;
using NotificationService.Application.Common;
using NotificationService.Domain.Common;
using NotificationService.Infrastructure.Common;
using NotificationService.Scheduler.Common;
using NotificationService.Worker.Common;

namespace NotificationService.Architecture.Tests;

public sealed class DependencyRulesTests
{
    [Fact]
    public void Domain_ShouldNotDependOnOtherLayers()
    {
        var result = Types
            .InAssembly(typeof(DomainAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "NotificationService.Application",
                "NotificationService.Infrastructure",
                "NotificationService.Api",
                "NotificationService.Worker",
                "NotificationService.Scheduler")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_ShouldDependOnlyOnDomain()
    {
        var result = Types
            .InAssembly(typeof(ApplicationAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "NotificationService.Infrastructure",
                "NotificationService.Api",
                "NotificationService.Worker",
                "NotificationService.Scheduler")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnHosts()
    {
        var result = Types
            .InAssembly(typeof(InfrastructureAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "NotificationService.Api",
                "NotificationService.Worker",
                "NotificationService.Scheduler")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Hosts_ShouldBeLoadable()
    {
        Assert.NotNull(typeof(ApiAssemblyMarker).Assembly);
        Assert.NotNull(typeof(WorkerAssemblyMarker).Assembly);
        Assert.NotNull(typeof(SchedulerAssemblyMarker).Assembly);
    }
}
