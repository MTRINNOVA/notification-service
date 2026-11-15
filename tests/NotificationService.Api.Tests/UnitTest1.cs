using NotificationService.Api.Common;

namespace NotificationService.Api.Tests;

public sealed class ApiFoundationTests
{
    [Fact]
    public void ApiAssemblyMarker_ShouldExist()
    {
        var markerType = typeof(ApiAssemblyMarker);
        Assert.NotNull(markerType);
    }
}
