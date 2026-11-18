using NotificationService.Domain.Entities;

namespace NotificationService.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IOrganizationRepository
{
    Task<Organization?> FindByIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);
}

public interface IApplicationRepository
{
    Task<ApplicationClient?> FindByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task AddAsync(ApplicationClient application, CancellationToken cancellationToken = default);
}

public interface INotificationRepository
{
    Task<Notification?> FindByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
}

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int take, DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
}

public interface IIdempotencyRecordRepository
{
    Task<IdempotencyRecord?> FindByKeyAsync(Guid applicationId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);
}
