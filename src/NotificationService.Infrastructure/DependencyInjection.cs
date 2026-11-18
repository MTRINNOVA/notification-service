using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NotificationService.Application.Abstractions.Persistence;
using NotificationService.Application.Abstractions.Time;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Infrastructure.Time;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Configure(options =>
                options.ConnectionString = configuration.GetConnectionString(DatabaseOptions.ConnectionStringName) ?? string.Empty)
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>());

        services.AddDbContext<NotificationDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(databaseOptions.ConnectionString);
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IIdempotencyRecordRepository, IdempotencyRecordRepository>();

        services.AddSingleton<IClock, SystemClock>();

        // Punto de registro para comprobaciones reales de dependencias (p.ej. PostgreSQL en Fase 2).
        // No se registra ninguna comprobación que declare saludable una dependencia no verificada.
        services.AddHealthChecks();

        return services;
    }
}
