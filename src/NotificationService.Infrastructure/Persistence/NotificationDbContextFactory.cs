using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Infrastructure.Persistence;

internal sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=notification_service;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
