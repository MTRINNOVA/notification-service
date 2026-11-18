using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    public static PropertyBuilder<TEnum> EnumAsString<TEnum>(this PropertyBuilder<TEnum> propertyBuilder, int maxLength)
        where TEnum : struct, Enum
    {
        return propertyBuilder.HasConversion<string>().HasMaxLength(maxLength);
    }
}

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

internal sealed class ApplicationConfiguration : IEntityTypeConfiguration<ApplicationClient>
{
    public void Configure(EntityTypeBuilder<ApplicationClient> builder)
    {
        builder.ToTable("applications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmailNormalized).HasMaxLength(320).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasIndex(x => x.EmailNormalized).IsUnique();
    }
}

internal sealed class OrganizationMembershipConfiguration : IEntityTypeConfiguration<OrganizationMembership>
{
    public void Configure(EntityTypeBuilder<OrganizationMembership> builder)
    {
        builder.ToTable("organization_memberships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasIndex(x => new { x.OrganizationId, x.UserId }).IsUnique();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).EnumAsString(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

internal sealed class MembershipRoleConfiguration : IEntityTypeConfiguration<MembershipRole>
{
    public void Configure(EntityTypeBuilder<MembershipRole> builder)
    {
        builder.ToTable("membership_roles");
        builder.HasKey(x => new { x.MembershipId, x.RoleId });
        builder.HasOne<OrganizationMembership>().WithMany().HasForeignKey(x => x.MembershipId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(x => new { x.RoleId, x.PermissionId });
        builder.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Permission>().WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PlatformRoleAssignmentConfiguration : IEntityTypeConfiguration<PlatformRoleAssignment>
{
    public void Configure(EntityTypeBuilder<PlatformRoleAssignment> builder)
    {
        builder.ToTable("platform_role_assignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RoleCode).EnumAsString(32).IsRequired();
        builder.Property(x => x.GrantedByActorType).EnumAsString(32).IsRequired();
        builder.Property(x => x.GrantReason).HasMaxLength(512).IsRequired();
        builder.Property(x => x.RevocationReason).HasMaxLength(512);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.GrantedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.RevokedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.UserId, x.RoleCode }).IsUnique().HasFilter("\"RevokedAt\" IS NULL");
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.CsrfTokenHash).HasMaxLength(128).IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.FamilyId);
    }
}

internal sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.KeyPrefix).HasMaxLength(32).IsRequired();
        builder.Property(x => x.KeyHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ScopesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApiKey>().WithMany().HasForeignKey(x => x.RotatedFromApiKeyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.KeyPrefix).IsUnique();
        builder.HasIndex(x => x.ApplicationId);
    }
}

internal sealed class ApplicationPermissionConfiguration : IEntityTypeConfiguration<ApplicationPermission>
{
    public void Configure(EntityTypeBuilder<ApplicationPermission> builder)
    {
        builder.ToTable("application_permissions");
        builder.HasKey(x => new { x.ApplicationId, x.PermissionCode });
        builder.Property(x => x.PermissionCode).HasMaxLength(128).IsRequired();
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ApplicationLimitConfiguration : IEntityTypeConfiguration<ApplicationLimit>
{
    public void Configure(EntityTypeBuilder<ApplicationLimit> builder)
    {
        builder.ToTable("application_limits");
        builder.HasKey(x => new { x.ApplicationId, x.Channel });
        builder.Property(x => x.Channel).EnumAsString(16).IsRequired();
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OrganizationLimitConfiguration : IEntityTypeConfiguration<OrganizationLimit>
{
    public void Configure(EntityTypeBuilder<OrganizationLimit> builder)
    {
        builder.ToTable("organization_limits");
        builder.HasKey(x => new { x.OrganizationId, x.Channel });
        builder.Property(x => x.Channel).EnumAsString(16).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProviderDefinitionConfiguration : IEntityTypeConfiguration<ProviderDefinition>
{
    public void Configure(EntityTypeBuilder<ProviderDefinition> builder)
    {
        builder.ToTable("provider_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).EnumAsString(32).IsRequired();
        builder.Property(x => x.Channel).EnumAsString(16).IsRequired();
        builder.Property(x => x.AdapterKey).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

internal sealed class ProviderAccountConfiguration : IEntityTypeConfiguration<ProviderAccount>
{
    public void Configure(EntityTypeBuilder<ProviderAccount> builder)
    {
        builder.ToTable("provider_accounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ConfigurationJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.Property(x => x.Channel).EnumAsString(16).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProviderDefinition>().WithMany().HasForeignKey(x => x.ProviderDefinitionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.Channel }).IsUnique().HasFilter("\"IsDefault\" = TRUE AND \"DeletedAt\" IS NULL");
    }
}

internal sealed class ProviderCredentialConfiguration : IEntityTypeConfiguration<ProviderCredential>
{
    public void Configure(EntityTypeBuilder<ProviderCredential> builder)
    {
        builder.ToTable("provider_credentials");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SecretReference).HasMaxLength(400);
        builder.Property(x => x.Ciphertext).HasColumnType("text");
        builder.Property(x => x.KeyVersion).HasMaxLength(64).IsRequired();
        builder.HasOne<ProviderAccount>().WithMany().HasForeignKey(x => x.ProviderAccountId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ProviderInstanceConfiguration : IEntityTypeConfiguration<ProviderInstance>
{
    public void Configure(EntityTypeBuilder<ProviderInstance> builder)
    {
        builder.ToTable("provider_instances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalInstanceId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ConnectionStatus).EnumAsString(32).IsRequired();
        builder.Property(x => x.SessionReference).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProviderAccount>().WithMany().HasForeignKey(x => x.ProviderAccountId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ProviderAccountId, x.ExternalInstanceId }).IsUnique();
    }
}

internal sealed class WhatsAppSenderConfiguration : IEntityTypeConfiguration<WhatsAppSender>
{
    public void Configure(EntityTypeBuilder<WhatsAppSender> builder)
    {
        builder.ToTable("whatsapp_senders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DisplayNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.NormalizedNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProviderAccount>().WithMany().HasForeignKey(x => x.ProviderAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProviderInstance>().WithMany().HasForeignKey(x => x.ProviderInstanceId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ApplicationSenderConfiguration : IEntityTypeConfiguration<ApplicationSender>
{
    public void Configure(EntityTypeBuilder<ApplicationSender> builder)
    {
        builder.ToTable("application_senders");
        builder.HasKey(x => new { x.ApplicationId, x.SenderId });
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<WhatsAppSender>().WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ApplicationId).IsUnique().HasFilter("\"IsDefault\" = TRUE");
    }
}

internal sealed class SmsGatewayConfiguration : IEntityTypeConfiguration<SmsGateway>
{
    public void Configure(EntityTypeBuilder<SmsGateway> builder)
    {
        builder.ToTable("sms_gateways");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.DeviceIdentifier).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SimSlot).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProviderAccount>().WithMany().HasForeignKey(x => x.ProviderAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ProviderAccountId).IsUnique();
    }
}

internal sealed class ProviderHealthCheckConfiguration : IEntityTypeConfiguration<ProviderHealthCheck>
{
    public void Configure(EntityTypeBuilder<ProviderHealthCheck> builder)
    {
        builder.ToTable("provider_health_checks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.Property(x => x.DetailsRedacted).HasColumnType("jsonb").IsRequired();
        builder.HasOne<ProviderAccount>().WithMany().HasForeignKey(x => x.ProviderAccountId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class NotificationBatchConfiguration : IEntityTypeConfiguration<NotificationBatch>
{
    public void Configure(EntityTypeBuilder<NotificationBatch> builder)
    {
        builder.ToTable("notification_batches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.Property(x => x.RejectedItemsJson).HasColumnType("jsonb").IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ApplicationId, x.CreatedAt });
        builder.ToTable(table => table.HasCheckConstraint("ck_notification_batches_counts", "\"AcceptedCount\" + \"RejectedCount\" = \"TotalCount\""));
    }
}

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Channel).EnumAsString(16).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.Property(x => x.RecipientJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ContentJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.TimezoneSnapshot).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ResendReason).HasMaxLength(512);
        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");
        builder.Property(x => x.EncryptionMode).EnumAsString(32).IsRequired();
        builder.Property(x => x.EncryptionPublicKey).HasColumnType("text");
        builder.Property(x => x.EncryptionSecretCiphertext).HasColumnType("text");
        builder.Property(x => x.EncryptionKeyVersion).HasMaxLength(64);
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Notification>().WithMany().HasForeignKey(x => x.ResendOfNotificationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApiKey>().WithMany().HasForeignKey(x => x.ResendRequestedByApiKeyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<NotificationBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TemplateVersion>().WithMany().HasForeignKey(x => x.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OrganizationId, x.CreatedAt });
        builder.HasIndex(x => new { x.ApplicationId, x.CreatedAt });
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => x.ResendOfNotificationId);
    }
}

internal sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("notification_deliveries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).EnumAsString(48).IsRequired();
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.HasOne<Notification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProviderAccount>().WithMany().HasForeignKey(x => x.ProviderAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WhatsAppSender>().WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmsGateway>().WithMany().HasForeignKey(x => x.SmsGatewayId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.NotificationId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.ProviderAccountId, x.ProviderMessageId });
    }
}

internal sealed class NotificationAttemptConfiguration : IEntityTypeConfiguration<NotificationAttempt>
{
    public void Configure(EntityTypeBuilder<NotificationAttempt> builder)
    {
        builder.ToTable("notification_attempts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ResultCode).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ErrorCode).HasMaxLength(128);
        builder.Property(x => x.ErrorDetailRedacted).HasMaxLength(2000);
        builder.Property(x => x.ProviderRequestId).HasMaxLength(200);
        builder.HasOne<NotificationDelivery>().WithMany().HasForeignKey(x => x.DeliveryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.DeliveryId, x.AttemptNumber }).IsUnique();
    }
}

internal sealed class DeliveryStatusHistoryConfiguration : IEntityTypeConfiguration<DeliveryStatusHistory>
{
    public void Configure(EntityTypeBuilder<DeliveryStatusHistory> builder)
    {
        builder.ToTable("delivery_status_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PreviousStatus).HasConversion<string>().HasMaxLength(48);
        builder.Property(x => x.NewStatus).EnumAsString(48).IsRequired();
        builder.Property(x => x.ActorType).EnumAsString(32).IsRequired();
        builder.Property(x => x.ReasonCode).HasMaxLength(128);
        builder.HasOne<NotificationDelivery>().WithMany().HasForeignKey(x => x.DeliveryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.DeliveryId, x.ChangedAt });
    }
}

internal sealed class NotificationStatusHistoryConfiguration : IEntityTypeConfiguration<NotificationStatusHistory>
{
    public void Configure(EntityTypeBuilder<NotificationStatusHistory> builder)
    {
        builder.ToTable("notification_status_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PreviousStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.NewStatus).EnumAsString(32).IsRequired();
        builder.Property(x => x.ActorType).EnumAsString(32).IsRequired();
        builder.HasOne<Notification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.NotificationId, x.ChangedAt });
    }
}

internal sealed class NotificationEventConfiguration : IEntityTypeConfiguration<NotificationEvent>
{
    public void Configure(EntityTypeBuilder<NotificationEvent> builder)
    {
        builder.ToTable("notification_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExternalEventId).HasMaxLength(200);
        builder.Property(x => x.PayloadRedacted).HasColumnType("jsonb").IsRequired();
        builder.HasOne<Notification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RequestHash).HasMaxLength(128).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ApplicationId, x.IdempotencyKey }).IsUnique();
    }
}

internal sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("templates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Channel).EnumAsString(16).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ApplicationId, x.Code, x.Channel }).IsUnique();
    }
}

internal sealed class TemplateVersionConfiguration : IEntityTypeConfiguration<TemplateVersion>
{
    public void Configure(EntityTypeBuilder<TemplateVersion> builder)
    {
        builder.ToTable("template_versions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Locale).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Body).HasColumnType("text").IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(200);
        builder.Property(x => x.SchemaJson).HasColumnType("jsonb");
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasOne<Template>().WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TemplateId, x.Version, x.Locale }).IsUnique();
    }
}

internal sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("attachments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ObjectKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ChecksumSha256).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ScanStatus).EnumAsString(32).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class NotificationAttachmentConfiguration : IEntityTypeConfiguration<NotificationAttachment>
{
    public void Configure(EntityTypeBuilder<NotificationAttachment> builder)
    {
        builder.ToTable("notification_attachments");
        builder.HasKey(x => new { x.NotificationId, x.AttachmentId });
        builder.HasOne<Notification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Attachment>().WithMany().HasForeignKey(x => x.AttachmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.AttachmentId);
    }
}

internal sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Channel).EnumAsString(16).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationClient>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TemplateVersion>().WithMany().HasForeignKey(x => x.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CampaignRecipientConfiguration : IEntityTypeConfiguration<CampaignRecipient>
{
    public void Configure(EntityTypeBuilder<CampaignRecipient> builder)
    {
        builder.ToTable("campaign_recipients");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RecipientRef).HasMaxLength(180).IsRequired();
        builder.Property(x => x.RecipientJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.Property(x => x.ErrorCode).HasMaxLength(128);
        builder.HasOne<Campaign>().WithMany().HasForeignKey(x => x.CampaignId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Notification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ScheduledNotificationConfiguration : IEntityTypeConfiguration<ScheduledNotification>
{
    public void Configure(EntityTypeBuilder<ScheduledNotification> builder)
    {
        builder.ToTable("scheduled_notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).EnumAsString(32).IsRequired();
        builder.Property(x => x.FailureCode).HasMaxLength(128);
        builder.HasOne<Notification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.Status, x.ExecuteAtUtc });
    }
}

internal sealed class QuotaUsageConfiguration : IEntityTypeConfiguration<QuotaUsage>
{
    public void Configure(EntityTypeBuilder<QuotaUsage> builder)
    {
        builder.ToTable("quota_usage");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Channel).EnumAsString(16).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OrganizationId, x.Channel, x.PeriodStartUtc }).IsUnique();
    }
}

internal sealed class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("webhook_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProviderEventId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ProcessingStatus).EnumAsString(32).IsRequired();
        builder.Property(x => x.PayloadRedacted).HasColumnType("jsonb").IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProviderAccount>().WithMany().HasForeignKey(x => x.ProviderAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ProviderAccountId, x.ProviderEventId }).IsUnique();
    }
}

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AggregateType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PublishedAt, x.NextAttemptAt });
    }
}

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConsumerName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.MessageId).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.ConsumerName, x.MessageId }).IsUnique();
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorType).EnumAsString(32).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ResourceType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Result).HasMaxLength(80).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.MetadataRedacted).HasColumnType("jsonb").IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApiKey>().WithMany().HasForeignKey(x => x.ApiKeyId).OnDelete(DeleteBehavior.Restrict);
    }
}
