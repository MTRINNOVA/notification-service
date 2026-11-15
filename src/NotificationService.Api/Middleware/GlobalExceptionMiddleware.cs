using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Api.Middleware;

/// <summary>
/// Convierte excepciones no controladas en RFC 9457 Problem Details sin exponer detalles internos.
/// Los errores funcionales del contrato (validación, negocio, cuota...) se implementan en sus fases.
/// </summary>
public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public const string ProblemJsonContentType = "application/problem+json";

    public const string InternalErrorCode = "INTERNAL_SERVER_ERROR";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión: no es un error del servidor y no hay a quién responder.
            logger.LogInformation(
                "Request {Method} {Path} was cancelled by the client. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                GetTraceId(context));
        }
        catch (Exception exception)
        {
            var traceId = GetTraceId(context);

            if (context.Response.HasStarted)
            {
                logger.LogError(
                    exception,
                    "Unhandled exception after the response started for {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    traceId);
                throw;
            }

            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                traceId);

            await WriteProblemAsync(context, traceId);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, string traceId)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Unexpected error",
            Detail = "An unexpected error occurred while processing the request.",
            Instance = context.Request.Path
        };
        problemDetails.Extensions["code"] = InternalErrorCode;
        problemDetails.Extensions["traceId"] = traceId;

        // El content type se pasa explícitamente: WriteAsJsonAsync lo sobrescribiría con application/json.
        await context.Response.WriteAsJsonAsync(
            problemDetails,
            (JsonSerializerOptions?)null,
            ProblemJsonContentType,
            context.RequestAborted);
    }

    private static string GetTraceId(HttpContext context) => Activity.Current?.Id ?? context.TraceIdentifier;
}
