using NotificationService.Domain.Common;

namespace NotificationService.Domain.Tests;

public sealed class DomainFoundationTests
{
    [Fact]
    public void DomainAssemblyMarker_ShouldExist()
    {
        var markerType = typeof(DomainAssemblyMarker);
        Assert.NotNull(markerType);
    }
}
