using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Abstractions.Persistence;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Repositories;

internal sealed class EfUnitOfWork(NotificationDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class OrganizationRepository(NotificationDbContext dbContext) : IOrganizationRepository
{
    public Task<Organization?> FindByIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return dbContext.Organizations.FirstOrDefaultAsync(x => x.Id == organizationId, cancellationToken);
    }

    public Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        return dbContext.Organizations.AddAsync(organization, cancellationToken).AsTask();
    }
}

internal sealed class ApplicationRepository(NotificationDbContext dbContext) : IApplicationRepository
{
    public Task<ApplicationClient?> FindByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return dbContext.Applications.FirstOrDefaultAsync(x => x.Id == applicationId, cancellationToken);
    }

    public Task AddAsync(ApplicationClient application, CancellationToken cancellationToken = default)
    {
        return dbContext.Applications.AddAsync(application, cancellationToken).AsTask();
    }
}

internal sealed class NotificationRepository(NotificationDbContext dbContext) : INotificationRepository
{
    public Task<Notification?> FindByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
    }

    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        return dbContext.Notifications.AddAsync(notification, cancellationToken).AsTask();
    }
}

internal sealed class OutboxRepository(NotificationDbContext dbContext) : IOutboxRepository
{
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        return dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int take, DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        var items = await dbContext.OutboxMessages
            .Where(x => x.PublishedAt == null && (x.NextAttemptAt == null || x.NextAttemptAt <= nowUtc))
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return items;
    }
}

internal sealed class IdempotencyRecordRepository(NotificationDbContext dbContext) : IIdempotencyRecordRepository
{
    public Task<IdempotencyRecord?> FindByKeyAsync(Guid applicationId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return dbContext.IdempotencyRecords
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        return dbContext.IdempotencyRecords.AddAsync(record, cancellationToken).AsTask();
    }
}
