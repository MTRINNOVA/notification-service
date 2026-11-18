using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace NotificationService.Infrastructure.Tests;

[CollectionDefinition(CollectionName)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlIntegrationFixture>
{
    public const string CollectionName = "postgresql-integration";
}

public sealed class PostgreSqlIntegrationFixture : IAsyncLifetime
{
    private const string PostgreSqlImage = "postgres@sha256:0d938e975177bef9f1f61f3f9dbcf2e0aa0a139568f66d3415ed8892eb97f6ac";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage(PostgreSqlImage)
        .WithDatabase("notification_service_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public NotificationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new NotificationDbContext(options);
    }
}
