using System.Reflection;
using NetArchTest.Rules;
using NotificationService.Api.Common;
using NotificationService.Application.Common;
using NotificationService.Domain.Common;
using NotificationService.Infrastructure.Common;
using NotificationService.Scheduler.Common;
using NotificationService.Worker.Common;

namespace NotificationService.Architecture.Tests;

/// <summary>
/// Reglas de dependencia a nivel de tipos (AGENTS.md §6 y §7). Complementan <see cref="ProjectStructureRulesTests"/>,
/// que valida las referencias declaradas en los .csproj.
/// </summary>
public sealed class DependencyRulesTests
{
    private static readonly string[] InfrastructureFrameworks =
    [
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "RabbitMQ",
        "StackExchange.Redis",
        "Minio",
        "MailKit",
        "Microsoft.AspNetCore"
    ];

    [Fact]
    public void Domain_ShouldNotDependOnOtherLayers()
    {
        AssertNoDependency(
            typeof(DomainAssemblyMarker).Assembly,
            "NotificationService.Application",
            "NotificationService.Infrastructure",
            "NotificationService.Api",
            "NotificationService.Worker",
            "NotificationService.Scheduler");
    }

    [Fact]
    public void Domain_ShouldNotDependOnInfrastructureFrameworks()
    {
        AssertNoDependency(typeof(DomainAssemblyMarker).Assembly, InfrastructureFrameworks);
    }

    [Fact]
    public void Application_ShouldDependOnlyOnDomain()
    {
        AssertNoDependency(
            typeof(ApplicationAssemblyMarker).Assembly,
            "NotificationService.Infrastructure",
            "NotificationService.Api",
            "NotificationService.Worker",
            "NotificationService.Scheduler");
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureFrameworks()
    {
        AssertNoDependency(typeof(ApplicationAssemblyMarker).Assembly, InfrastructureFrameworks);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnHosts()
    {
        AssertNoDependency(
            typeof(InfrastructureAssemblyMarker).Assembly,
            "NotificationService.Api",
            "NotificationService.Worker",
            "NotificationService.Scheduler");
    }

    [Theory]
    [InlineData(typeof(ApiAssemblyMarker), "NotificationService.Worker", "NotificationService.Scheduler")]
    [InlineData(typeof(WorkerAssemblyMarker), "NotificationService.Api", "NotificationService.Scheduler")]
    [InlineData(typeof(SchedulerAssemblyMarker), "NotificationService.Api", "NotificationService.Worker")]
    public void Hosts_ShouldNotDependOnEachOther(Type hostMarker, params string[] otherHosts)
    {
        AssertNoDependency(hostMarker.Assembly, otherHosts);
    }

    [Fact]
    public void Hosts_ShouldBeLoadable()
    {
        Assert.NotNull(typeof(ApiAssemblyMarker).Assembly);
        Assert.NotNull(typeof(WorkerAssemblyMarker).Assembly);
        Assert.NotNull(typeof(SchedulerAssemblyMarker).Assembly);
    }

    private static void AssertNoDependency(Assembly assembly, params string[] forbiddenNamespaces)
    {
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"{assembly.GetName().Name} has forbidden dependencies in: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
