using NotificationService.Worker.Common;

namespace NotificationService.Worker.Tests;

public sealed class WorkerFoundationTests
{
    [Fact]
    public void WorkerAssemblyMarker_ShouldExist()
    {
        var markerType = typeof(WorkerAssemblyMarker);
        Assert.NotNull(markerType);
    }
}
