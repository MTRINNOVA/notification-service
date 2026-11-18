using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

public sealed class Organization
{
    private Organization() { }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public string Timezone { get; private set; } = "America/Lima";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
}

public sealed class ApplicationClient
{
    private ApplicationClient() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
}

public sealed class User
{
    private User() { }

    public Guid Id { get; private set; }
    public string EmailNormalized { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public bool MustChangePassword { get; private set; }
    public DateTimeOffset? PasswordChangedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
}

public sealed class OrganizationMembership
{
    private OrganizationMembership() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public TenantStatus Status { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
}

public sealed class Role
{
    private Role() { }

    public Guid Id { get; private set; }
    public RoleCode Code { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
}

public sealed class Permission
{
    private Permission() { }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
}

public sealed class MembershipRole
{
    private MembershipRole() { }

    public Guid MembershipId { get; private set; }
    public Guid RoleId { get; private set; }
}

public sealed class RolePermission
{
    private RolePermission() { }

    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
}

public sealed class PlatformRoleAssignment
{
    private PlatformRoleAssignment() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public PlatformRoleCode RoleCode { get; private set; }
    public GrantActorType GrantedByActorType { get; private set; }
    public Guid? GrantedBy { get; private set; }
    public string GrantReason { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? RevokedBy { get; private set; }
    public string? RevocationReason { get; private set; }
}

public sealed class RefreshToken
{
    private RefreshToken() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public Guid FamilyId { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string CsrfTokenHash { get; private set; } = string.Empty;
}

public sealed class ApiKey
{
    private ApiKey() { }

    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string KeyPrefix { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public string ScopesJson { get; private set; } = "[]";
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? RotatedFromApiKeyId { get; private set; }
}

public sealed class ApplicationPermission
{
    private ApplicationPermission() { }

    public Guid ApplicationId { get; private set; }
    public string PermissionCode { get; private set; } = string.Empty;
}

public sealed class ApplicationLimit
{
    private ApplicationLimit() { }

    public Guid ApplicationId { get; private set; }
    public ChannelType Channel { get; private set; }
    public int? DailyLimit { get; private set; }
    public int? RatePerMinute { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
}

public sealed class OrganizationLimit
{
    private OrganizationLimit() { }

    public Guid OrganizationId { get; private set; }
    public ChannelType Channel { get; private set; }
    public int? DailyLimit { get; private set; }
    public int? RatePerMinute { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
}

public sealed class ProviderDefinition
{
    private ProviderDefinition() { }

    public Guid Id { get; private set; }
    public ProviderDefinitionCode Code { get; private set; }
    public ChannelType Channel { get; private set; }
    public string AdapterKey { get; private set; } = string.Empty;
    public bool Enabled { get; private set; }
}

public sealed class ProviderAccount
{
    private ProviderAccount() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ProviderDefinitionId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ProviderAccountStatus Status { get; private set; }
    public string ConfigurationJson { get; private set; } = "{}";
    public bool IsDefault { get; private set; }
    public ChannelType Channel { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
}

public sealed class ProviderCredential
{
    private ProviderCredential() { }

    public Guid Id { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public string? SecretReference { get; private set; }
    public string? Ciphertext { get; private set; }
    public string KeyVersion { get; private set; } = string.Empty;
    public DateTimeOffset RotatedAt { get; private set; }
}

public sealed class ProviderInstance
{
    private ProviderInstance() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public string ExternalInstanceId { get; private set; } = string.Empty;
    public ProviderConnectionStatus ConnectionStatus { get; private set; }
    public string SessionReference { get; private set; } = string.Empty;
    public DateTimeOffset? LastConnectedAt { get; private set; }
    public DateTimeOffset? LastSeenAt { get; private set; }
    public string Name { get; private set; } = string.Empty;
}

public sealed class WhatsAppSender
{
    private WhatsAppSender() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public Guid ProviderInstanceId { get; private set; }
    public string DisplayNumber { get; private set; } = string.Empty;
    public string NormalizedNumber { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public SenderStatus Status { get; private set; }
    public DateTimeOffset? ConnectedAt { get; private set; }
    public DateTimeOffset? LastActivityAt { get; private set; }
}

public sealed class ApplicationSender
{
    private ApplicationSender() { }

    public Guid ApplicationId { get; private set; }
    public Guid SenderId { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class SmsGateway
{
    private SmsGateway() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public string DeviceLabel { get; private set; } = string.Empty;
    public string DeviceIdentifier { get; private set; } = string.Empty;
    public string SimSlot { get; private set; } = string.Empty;
    public SmsGatewayStatus Status { get; private set; }
    public DateTimeOffset? LastSeenAt { get; private set; }
}

public sealed class ProviderHealthCheck
{
    private ProviderHealthCheck() { }

    public Guid Id { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public ProviderHealthStatus Status { get; private set; }
    public int? LatencyMs { get; private set; }
    public DateTimeOffset CheckedAt { get; private set; }
    public string DetailsRedacted { get; private set; } = "{}";
}

public sealed class NotificationBatch
{
    private NotificationBatch() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public NotificationBatchStatus Status { get; private set; }
    public int TotalCount { get; private set; }
    public int AcceptedCount { get; private set; }
    public int RejectedCount { get; private set; }
    public string RejectedItemsJson { get; private set; } = "[]";
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class Notification
{
    private Notification() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public ChannelType Channel { get; private set; }
    public string RecipientJson { get; private set; } = "{}";
    public string ContentJson { get; private set; } = "{}";
    public NotificationPublicStatus Status { get; private set; }
    public DateTimeOffset? ScheduledAt { get; private set; }
    public string TimezoneSnapshot { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public Guid? ResendOfNotificationId { get; private set; }
    public string? ResendReason { get; private set; }
    public Guid? ResendRequestedByApiKeyId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset AcceptedAt { get; private set; }
    public Guid? BatchId { get; private set; }
    public Guid? TemplateVersionId { get; private set; }
    public string? MetadataJson { get; private set; }
    public EncryptionMode EncryptionMode { get; private set; }
    public string? EncryptionPublicKey { get; private set; }
    public string? EncryptionSecretCiphertext { get; private set; }
    public string? EncryptionKeyVersion { get; private set; }

    public void MoveTo(NotificationPublicStatus targetStatus)
    {
        Status = targetStatus;
    }
}

public sealed class NotificationDelivery
{
    private NotificationDelivery() { }

    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public Guid? SenderId { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public DateTimeOffset? AcceptedByProviderAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public Guid? SmsGatewayId { get; private set; }
    public DateTimeOffset? ReconciliationStartedAt { get; private set; }
    public DateTimeOffset? ReconciliationDeadlineAt { get; private set; }
    public DateTimeOffset? NextReconciliationAt { get; private set; }
    public int? ReconciliationChecks { get; private set; }
    public bool RequiresManualIntervention { get; private set; }

    public void MoveTo(DeliveryStatus targetStatus)
    {
        Status = targetStatus;
    }
}

public sealed class NotificationAttempt
{
    private NotificationAttempt() { }

    public Guid Id { get; private set; }
    public Guid DeliveryId { get; private set; }
    public int AttemptNumber { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public AttemptResultCode? ResultCode { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorDetailRedacted { get; private set; }
    public string? ProviderRequestId { get; private set; }
}

public sealed class DeliveryStatusHistory
{
    private DeliveryStatusHistory() { }

    public Guid Id { get; private set; }
    public Guid DeliveryId { get; private set; }
    public DeliveryStatus? PreviousStatus { get; private set; }
    public DeliveryStatus NewStatus { get; private set; }
    public NotificationActorType ActorType { get; private set; }
    public Guid? ActorId { get; private set; }
    public string? ReasonCode { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
}

public sealed class NotificationStatusHistory
{
    private NotificationStatusHistory() { }

    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public NotificationPublicStatus? PreviousStatus { get; private set; }
    public NotificationPublicStatus NewStatus { get; private set; }
    public NotificationActorType ActorType { get; private set; }
    public Guid? ActorId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
}

public sealed class NotificationEvent
{
    private NotificationEvent() { }

    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string? ExternalEventId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string PayloadRedacted { get; private set; } = "{}";
}

public sealed class IdempotencyRecord
{
    private IdempotencyRecord() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public Guid ResourceId { get; private set; }
    public int ResponseStatus { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
}

public sealed class Template
{
    private Template() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ChannelType Channel { get; private set; }
    public TemplateStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
}

public sealed class TemplateVersion
{
    private TemplateVersion() { }

    public Guid Id { get; private set; }
    public Guid TemplateId { get; private set; }
    public int Version { get; private set; }
    public string Locale { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public string? SchemaJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public TemplateVersionStatus Status { get; private set; }
}

public sealed class Attachment
{
    private Attachment() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string ObjectKey { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string ChecksumSha256 { get; private set; } = string.Empty;
    public AttachmentScanStatus ScanStatus { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
}

public sealed class NotificationAttachment
{
    private NotificationAttachment() { }

    public Guid NotificationId { get; private set; }
    public Guid AttachmentId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class Campaign
{
    private Campaign() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ChannelType Channel { get; private set; }
    public Guid TemplateVersionId { get; private set; }
    public CampaignStatus Status { get; private set; }
    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public sealed class CampaignRecipient
{
    private CampaignRecipient() { }

    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public string RecipientRef { get; private set; } = string.Empty;
    public string RecipientJson { get; private set; } = "{}";
    public CampaignRecipientStatus Status { get; private set; }
    public Guid? NotificationId { get; private set; }
    public string? ErrorCode { get; private set; }
}

public sealed class ScheduledNotification
{
    private ScheduledNotification() { }

    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public DateTimeOffset ExecuteAtUtc { get; private set; }
    public string Timezone { get; private set; } = string.Empty;
    public ScheduledNotificationStatus Status { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? FailureCode { get; private set; }
}

public sealed class QuotaUsage
{
    private QuotaUsage() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public ChannelType Channel { get; private set; }
    public DateTimeOffset PeriodStartUtc { get; private set; }
    public DateTimeOffset PeriodEndUtc { get; private set; }
    public int AcceptedCount { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
}

public sealed class WebhookEvent
{
    private WebhookEvent() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public string ProviderEventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public WebhookProcessingStatus ProcessingStatus { get; private set; }
    public string PayloadRedacted { get; private set; } = "{}";
}

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public int Attempts { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
}

public sealed class InboxMessage
{
    private InboxMessage() { }

    public Guid Id { get; private set; }
    public string ConsumerName { get; private set; } = string.Empty;
    public string MessageId { get; private set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
}

public sealed class AuditLog
{
    private AuditLog() { }

    public Guid Id { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public NotificationActorType ActorType { get; private set; }
    public Guid? ActorId { get; private set; }
    public Guid? ApiKeyId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string ResourceType { get; private set; } = string.Empty;
    public Guid? ResourceId { get; private set; }
    public string Result { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public string MetadataRedacted { get; private set; } = "{}";
}
