using NotificationService.Application;
using NotificationService.Infrastructure;

namespace NotificationService.Worker.Common;

/// <summary>
/// Composition root del host. Program.cs solo construye y ejecuta el host; las pruebas reutilizan este método.
/// </summary>
public static class WorkerHostBuilderExtensions
{
    public static HostApplicationBuilder AddWorkerHost(this HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options =>
        {
            // Mismo formato que la API (NotificationService.Api.Common.StructuredLogging).
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddHostedService<WorkerService>();

        return builder;
    }
}
