namespace NotificationService.Domain.Enums;

public enum TenantStatus
{
    Active = 1,
    Suspended = 2,
    Disabled = 3
}

public enum ChannelType
{
    WhatsApp = 1,
    Email = 2,
    Sms = 3
}

public enum RoleCode
{
    OrganizationAdmin = 1,
    Operator = 2,
    Viewer = 3
}

public enum PlatformRoleCode
{
    SuperAdmin = 1
}

public enum GrantActorType
{
    User = 1,
    SystemBootstrap = 2
}

public enum ProviderAccountStatus
{
    Active = 1,
    Disabled = 2
}

public enum ProviderConnectionStatus
{
    Created = 1,
    Connecting = 2,
    Connected = 3,
    Disconnected = 4
}

public enum SenderStatus
{
    Active = 1,
    Disconnected = 2,
    Disabled = 3
}

public enum SmsGatewayStatus
{
    Active = 1,
    Offline = 2,
    Disabled = 3
}

public enum ProviderHealthStatus
{
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3
}

public enum NotificationPublicStatus
{
    Accepted = 1,
    Queued = 2,
    Processing = 3,
    Sent = 4,
    Delivered = 5,
    Read = 6,
    Failed = 7,
    Cancelled = 8
}

public enum DeliveryStatus
{
    Pending = 1,
    Queued = 2,
    InProgress = 3,
    ProviderAccepted = 4,
    RetryPending = 5,
    ReconciliationRequired = 6,
    Delivered = 7,
    Read = 8,
    Failed = 9,
    DeadLetter = 10,
    Cancelled = 11
}

public enum AttemptResultCode
{
    ProviderAccepted = 1,
    TransientFailure = 2,
    PermanentFailure = 3,
    Ambiguous = 4
}

public enum NotificationBatchStatus
{
    Accepted = 1,
    PartiallyAccepted = 2,
    Rejected = 3
}

public enum TemplateStatus
{
    Active = 1,
    Disabled = 2
}

public enum TemplateVersionStatus
{
    Published = 1,
    Archived = 2
}

public enum CampaignStatus
{
    Draft = 1,
    Scheduled = 2,
    Running = 3,
    Paused = 4,
    Completed = 5,
    Cancelled = 6
}

public enum CampaignRecipientStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Cancelled = 4
}

public enum ScheduledNotificationStatus
{
    Pending = 1,
    Executed = 2,
    Cancelled = 3,
    Failed = 4
}

public enum WebhookProcessingStatus
{
    Received = 1,
    Processed = 2,
    Ignored = 3,
    Failed = 4
}

public enum AttachmentScanStatus
{
    Pending = 1,
    Clean = 2,
    Blocked = 3,
    NotScanned = 4
}

public enum EncryptionMode
{
    None = 1,
    TlsOnly = 2,
    ZipAes256 = 3,
    Pgp = 4
}

public enum NotificationActorType
{
    User = 1,
    Application = 2,
    System = 3,
    SystemBootstrap = 4,
    Provider = 5
}

public enum TemplateChannel
{
    WhatsApp = 1,
    Email = 2,
    Sms = 3
}

public enum ProviderDefinitionCode
{
    EvolutionApi = 1,
    PostfixSmtp = 2,
    CustomSmtp = 3,
    AndroidSmsGateway = 4
}
