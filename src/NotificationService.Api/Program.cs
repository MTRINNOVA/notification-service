using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationService.Api.Common;
using NotificationService.Api.Middleware;
using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(StructuredLogging.Configure);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API process is running."), tags: [HealthCheckTags.Live]);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Documento OpenAPI y Swagger UI: habilitados en Development; en otros entornos solo con OpenApi:Enabled=true.
// El contrato normativo sigue siendo docs/api/openapi.yaml; este documento describe lo implementado.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>(ApiDocumentation.EnabledConfigurationKey))
{
    app.MapOpenApi(ApiDocumentation.DocumentRoute);
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(ApiDocumentation.DocumentUrl, "Notification Service API v1");
        options.RoutePrefix = ApiDocumentation.SwaggerUiRoutePrefix;
    });
}

app.MapGet("/", () => Results.Ok(new
{
    service = "notification-service-api",
    status = "running"
}));

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Live)
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Ready)
});

app.Run();
