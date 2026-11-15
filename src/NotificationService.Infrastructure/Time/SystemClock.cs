using NotificationService.Application.Abstractions.Time;

namespace NotificationService.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}