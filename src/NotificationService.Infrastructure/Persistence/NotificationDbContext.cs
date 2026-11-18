using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
	public DbSet<Organization> Organizations => Set<Organization>();
	public DbSet<ApplicationClient> Applications => Set<ApplicationClient>();
	public DbSet<User> Users => Set<User>();
	public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();
	public DbSet<Role> Roles => Set<Role>();
	public DbSet<Permission> Permissions => Set<Permission>();
	public DbSet<MembershipRole> MembershipRoles => Set<MembershipRole>();
	public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
	public DbSet<PlatformRoleAssignment> PlatformRoleAssignments => Set<PlatformRoleAssignment>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
	public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
	public DbSet<ApplicationPermission> ApplicationPermissions => Set<ApplicationPermission>();
	public DbSet<ApplicationLimit> ApplicationLimits => Set<ApplicationLimit>();
	public DbSet<OrganizationLimit> OrganizationLimits => Set<OrganizationLimit>();
	public DbSet<ProviderDefinition> ProviderDefinitions => Set<ProviderDefinition>();
	public DbSet<ProviderAccount> ProviderAccounts => Set<ProviderAccount>();
	public DbSet<ProviderCredential> ProviderCredentials => Set<ProviderCredential>();
	public DbSet<ProviderInstance> ProviderInstances => Set<ProviderInstance>();
	public DbSet<WhatsAppSender> WhatsAppSenders => Set<WhatsAppSender>();
	public DbSet<ApplicationSender> ApplicationSenders => Set<ApplicationSender>();
	public DbSet<SmsGateway> SmsGateways => Set<SmsGateway>();
	public DbSet<ProviderHealthCheck> ProviderHealthChecks => Set<ProviderHealthCheck>();
	public DbSet<NotificationBatch> NotificationBatches => Set<NotificationBatch>();
	public DbSet<Notification> Notifications => Set<Notification>();
	public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();
	public DbSet<NotificationAttempt> NotificationAttempts => Set<NotificationAttempt>();
	public DbSet<DeliveryStatusHistory> DeliveryStatusHistories => Set<DeliveryStatusHistory>();
	public DbSet<NotificationStatusHistory> NotificationStatusHistories => Set<NotificationStatusHistory>();
	public DbSet<NotificationEvent> NotificationEvents => Set<NotificationEvent>();
	public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
	public DbSet<Template> Templates => Set<Template>();
	public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
	public DbSet<Attachment> Attachments => Set<Attachment>();
	public DbSet<NotificationAttachment> NotificationAttachments => Set<NotificationAttachment>();
	public DbSet<Campaign> Campaigns => Set<Campaign>();
	public DbSet<CampaignRecipient> CampaignRecipients => Set<CampaignRecipient>();
	public DbSet<ScheduledNotification> ScheduledNotifications => Set<ScheduledNotification>();
	public DbSet<QuotaUsage> QuotaUsages => Set<QuotaUsage>();
	public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
	public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
	public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
	public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
	}
}