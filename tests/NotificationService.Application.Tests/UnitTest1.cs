using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application;

namespace NotificationService.Application.Tests;

public sealed class ApplicationFoundationTests
{
    [Fact]
    public void AddApplication_ShouldReturnServiceCollection()
    {
        var services = new ServiceCollection();

        var returned = services.AddApplication();

        Assert.Same(services, returned);
    }
}
