using Microsoft.Extensions.Logging.Console;

namespace NotificationService.Api.Common;

/// <summary>
/// Formato de logs JSON común. Worker y Scheduler aplican las mismas opciones.
/// </summary>
internal static class StructuredLogging
{
    public static void Configure(JsonConsoleFormatterOptions options)
    {
        options.IncludeScopes = true;
        options.UseUtcTimestamp = true;
        options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    }
}
