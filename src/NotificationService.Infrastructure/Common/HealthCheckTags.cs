namespace NotificationService.Infrastructure.Common;

/// <summary>
/// Etiquetas para separar health checks de liveness y readiness.
/// Una dependencia solo se registra con <see cref="Ready"/> cuando existe una comprobación real de ella.
/// </summary>
public static class HealthCheckTags
{
    public const string Live = "live";

    public const string Ready = "ready";
}
