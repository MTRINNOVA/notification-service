using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "provider_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AdapterKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailNormalized = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_applications_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outbox_messages_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quota_usage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PeriodStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quota_usage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quota_usage_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provider_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_provider_accounts_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_accounts_provider_definitions_ProviderDefinitionId",
                        column: x => x.ProviderDefinitionId,
                        principalTable: "provider_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_limits",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DailyLimit = table.Column<int>(type: "integer", nullable: true),
                    RatePerMinute = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_limits", x => new { x.OrganizationId, x.Channel });
                    table.ForeignKey(
                        name: "FK_organization_limits_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_limits_users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_memberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organization_memberships_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_memberships_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_role_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GrantedByActorType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GrantedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    GrantReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_role_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_role_assignments_users_GrantedBy",
                        column: x => x.GrantedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_role_assignments_users_RevokedBy",
                        column: x => x.RevokedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_role_assignments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CsrfTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ScopesJson = table.Column<string>(type: "jsonb", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RotatedFromApiKeyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_api_keys_api_keys_RotatedFromApiKeyId",
                        column: x => x.RotatedFromApiKeyId,
                        principalTable: "api_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_api_keys_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_limits",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DailyLimit = table.Column<int>(type: "integer", nullable: true),
                    RatePerMinute = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_limits", x => new { x.ApplicationId, x.Channel });
                    table.ForeignKey(
                        name: "FK_application_limits_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_permissions",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_permissions", x => new { x.ApplicationId, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_application_permissions_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ChecksumSha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScanStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attachments_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_attachments_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "idempotency_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseStatus = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_idempotency_records_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_idempotency_records_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    AcceptedCount = table.Column<int>(type: "integer", nullable: false),
                    RejectedCount = table.Column<int>(type: "integer", nullable: false),
                    RejectedItemsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_batches", x => x.Id);
                    table.CheckConstraint("ck_notification_batches_counts", "\"AcceptedCount\" + \"RejectedCount\" = \"TotalCount\"");
                    table.ForeignKey(
                        name: "FK_notification_batches_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notification_batches_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_templates_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_templates_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provider_credentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecretReference = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Ciphertext = table.Column<string>(type: "text", nullable: true),
                    KeyVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RotatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_credentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_provider_credentials_provider_accounts_ProviderAccountId",
                        column: x => x.ProviderAccountId,
                        principalTable: "provider_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_health_checks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LatencyMs = table.Column<int>(type: "integer", nullable: true),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DetailsRedacted = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_health_checks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_provider_health_checks_provider_accounts_ProviderAccountId",
                        column: x => x.ProviderAccountId,
                        principalTable: "provider_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_instances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalInstanceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConnectionStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SessionReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastConnectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_instances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_provider_instances_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provider_instances_provider_accounts_ProviderAccountId",
                        column: x => x.ProviderAccountId,
                        principalTable: "provider_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sms_gateways",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SimSlot = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_gateways", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sms_gateways_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sms_gateways_provider_accounts_ProviderAccountId",
                        column: x => x.ProviderAccountId,
                        principalTable: "provider_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "webhook_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderEventId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcessingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PayloadRedacted = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_webhook_events_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_webhook_events_provider_accounts_ProviderAccountId",
                        column: x => x.ProviderAccountId,
                        principalTable: "provider_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membership_roles",
                columns: table => new
                {
                    MembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_roles", x => new { x.MembershipId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_membership_roles_organization_memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "organization_memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membership_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApiKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Result = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MetadataRedacted = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_api_keys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "api_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_logs_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "template_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Locale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SchemaJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_template_versions_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "whatsapp_senders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NormalizedNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConnectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastActivityAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whatsapp_senders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_whatsapp_senders_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_whatsapp_senders_provider_accounts_ProviderAccountId",
                        column: x => x.ProviderAccountId,
                        principalTable: "provider_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_whatsapp_senders_provider_instances_ProviderInstanceId",
                        column: x => x.ProviderInstanceId,
                        principalTable: "provider_instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TemplateVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_campaigns_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_campaigns_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_campaigns_template_versions_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalTable: "template_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RecipientJson = table.Column<string>(type: "jsonb", nullable: false),
                    ContentJson = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TimezoneSnapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResendOfNotificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResendReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ResendRequestedByApiKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    EncryptionMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EncryptionPublicKey = table.Column<string>(type: "text", nullable: true),
                    EncryptionSecretCiphertext = table.Column<string>(type: "text", nullable: true),
                    EncryptionKeyVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_api_keys_ResendRequestedByApiKeyId",
                        column: x => x.ResendRequestedByApiKeyId,
                        principalTable: "api_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_notification_batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "notification_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_notifications_ResendOfNotificationId",
                        column: x => x.ResendOfNotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_template_versions_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalTable: "template_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_senders",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_senders", x => new { x.ApplicationId, x.SenderId });
                    table.ForeignKey(
                        name: "FK_application_senders_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_senders_whatsapp_senders_SenderId",
                        column: x => x.SenderId,
                        principalTable: "whatsapp_senders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "campaign_recipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientRef = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    RecipientJson = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaign_recipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_campaign_recipients_campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_campaign_recipients_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_attachments",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_attachments", x => new { x.NotificationId, x.AttachmentId });
                    table.ForeignKey(
                        name: "FK_notification_attachments_attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notification_attachments_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AcceptedByProviderAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SmsGatewayId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReconciliationStartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReconciliationDeadlineAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextReconciliationAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReconciliationChecks = table.Column<int>(type: "integer", nullable: true),
                    RequiresManualIntervention = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_deliveries_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notification_deliveries_provider_accounts_ProviderAccountId",
                        column: x => x.ProviderAccountId,
                        principalTable: "provider_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notification_deliveries_sms_gateways_SmsGatewayId",
                        column: x => x.SmsGatewayId,
                        principalTable: "sms_gateways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notification_deliveries_whatsapp_senders_SenderId",
                        column: x => x.SenderId,
                        principalTable: "whatsapp_senders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadRedacted = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_events_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_status_history_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecuteAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scheduled_notifications_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "delivery_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    ActorType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_delivery_status_history_notification_deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "notification_deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResultCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ErrorDetailRedacted = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProviderRequestId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_attempts_notification_deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "notification_deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_ApplicationId",
                table: "api_keys",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_KeyPrefix",
                table: "api_keys",
                column: "KeyPrefix",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_RotatedFromApiKeyId",
                table: "api_keys",
                column: "RotatedFromApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_application_senders_ApplicationId",
                table: "application_senders",
                column: "ApplicationId",
                unique: true,
                filter: "\"IsDefault\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_application_senders_SenderId",
                table: "application_senders",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_applications_OrganizationId_Code",
                table: "applications",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attachments_ApplicationId",
                table: "attachments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_OrganizationId",
                table: "attachments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ApiKeyId",
                table: "audit_logs",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_OrganizationId",
                table: "audit_logs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_campaign_recipients_CampaignId",
                table: "campaign_recipients",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_campaign_recipients_NotificationId",
                table: "campaign_recipients",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_ApplicationId",
                table: "campaigns",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_OrganizationId",
                table: "campaigns",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_TemplateVersionId",
                table: "campaigns",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_status_history_DeliveryId_ChangedAt",
                table: "delivery_status_history",
                columns: new[] { "DeliveryId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_records_ApplicationId_IdempotencyKey",
                table: "idempotency_records",
                columns: new[] { "ApplicationId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_records_OrganizationId",
                table: "idempotency_records",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_ConsumerName_MessageId",
                table: "inbox_messages",
                columns: new[] { "ConsumerName", "MessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_membership_roles_RoleId",
                table: "membership_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_attachments_AttachmentId",
                table: "notification_attachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_attempts_DeliveryId_AttemptNumber",
                table: "notification_attempts",
                columns: new[] { "DeliveryId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_batches_ApplicationId_CreatedAt",
                table: "notification_batches",
                columns: new[] { "ApplicationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_batches_OrganizationId",
                table: "notification_batches",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_NotificationId",
                table: "notification_deliveries",
                column: "NotificationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_ProviderAccountId_ProviderMessageId",
                table: "notification_deliveries",
                columns: new[] { "ProviderAccountId", "ProviderMessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_SenderId",
                table: "notification_deliveries",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_SmsGatewayId",
                table: "notification_deliveries",
                column: "SmsGatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_deliveries_Status",
                table: "notification_deliveries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_notification_events_NotificationId",
                table: "notification_events",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_status_history_NotificationId_ChangedAt",
                table: "notification_status_history",
                columns: new[] { "NotificationId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_ApplicationId_CreatedAt",
                table: "notifications",
                columns: new[] { "ApplicationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_BatchId",
                table: "notifications",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_OrganizationId_CreatedAt",
                table: "notifications",
                columns: new[] { "OrganizationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_ResendOfNotificationId",
                table: "notifications",
                column: "ResendOfNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_ResendRequestedByApiKeyId",
                table: "notifications",
                column: "ResendRequestedByApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_Status_CreatedAt",
                table: "notifications",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_TemplateVersionId",
                table: "notifications",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_organization_limits_UpdatedBy",
                table: "organization_limits",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_OrganizationId_UserId",
                table: "organization_memberships",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_UserId",
                table: "organization_memberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_Code",
                table: "organizations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_OrganizationId",
                table: "outbox_messages",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_PublishedAt_NextAttemptAt",
                table: "outbox_messages",
                columns: new[] { "PublishedAt", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_permissions_Code",
                table: "permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_role_assignments_GrantedBy",
                table: "platform_role_assignments",
                column: "GrantedBy");

            migrationBuilder.CreateIndex(
                name: "IX_platform_role_assignments_RevokedBy",
                table: "platform_role_assignments",
                column: "RevokedBy");

            migrationBuilder.CreateIndex(
                name: "IX_platform_role_assignments_UserId_RoleCode",
                table: "platform_role_assignments",
                columns: new[] { "UserId", "RoleCode" },
                unique: true,
                filter: "\"RevokedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_provider_accounts_OrganizationId_Channel",
                table: "provider_accounts",
                columns: new[] { "OrganizationId", "Channel" },
                unique: true,
                filter: "\"IsDefault\" = TRUE AND \"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_provider_accounts_OrganizationId_Code",
                table: "provider_accounts",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_accounts_ProviderDefinitionId",
                table: "provider_accounts",
                column: "ProviderDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_provider_credentials_ProviderAccountId",
                table: "provider_credentials",
                column: "ProviderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_provider_definitions_Code",
                table: "provider_definitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_health_checks_ProviderAccountId",
                table: "provider_health_checks",
                column: "ProviderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_provider_instances_OrganizationId",
                table: "provider_instances",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_provider_instances_ProviderAccountId_ExternalInstanceId",
                table: "provider_instances",
                columns: new[] { "ProviderAccountId", "ExternalInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quota_usage_OrganizationId_Channel_PeriodStartUtc",
                table: "quota_usage",
                columns: new[] { "OrganizationId", "Channel", "PeriodStartUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_FamilyId",
                table: "refresh_tokens",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_PermissionId",
                table: "role_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Code",
                table: "roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_NotificationId",
                table: "scheduled_notifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_notifications_Status_ExecuteAtUtc",
                table: "scheduled_notifications",
                columns: new[] { "Status", "ExecuteAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_sms_gateways_OrganizationId",
                table: "sms_gateways",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_sms_gateways_ProviderAccountId",
                table: "sms_gateways",
                column: "ProviderAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_versions_TemplateId_Version_Locale",
                table: "template_versions",
                columns: new[] { "TemplateId", "Version", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_templates_ApplicationId_Code_Channel",
                table: "templates",
                columns: new[] { "ApplicationId", "Code", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_templates_OrganizationId",
                table: "templates",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_users_EmailNormalized",
                table: "users",
                column: "EmailNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_OrganizationId",
                table: "webhook_events",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_ProviderAccountId_ProviderEventId",
                table: "webhook_events",
                columns: new[] { "ProviderAccountId", "ProviderEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_senders_OrganizationId",
                table: "whatsapp_senders",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_senders_ProviderAccountId",
                table: "whatsapp_senders",
                column: "ProviderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_senders_ProviderInstanceId",
                table: "whatsapp_senders",
                column: "ProviderInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_limits");

            migrationBuilder.DropTable(
                name: "application_permissions");

            migrationBuilder.DropTable(
                name: "application_senders");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "campaign_recipients");

            migrationBuilder.DropTable(
                name: "delivery_status_history");

            migrationBuilder.DropTable(
                name: "idempotency_records");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "membership_roles");

            migrationBuilder.DropTable(
                name: "notification_attachments");

            migrationBuilder.DropTable(
                name: "notification_attempts");

            migrationBuilder.DropTable(
                name: "notification_events");

            migrationBuilder.DropTable(
                name: "notification_status_history");

            migrationBuilder.DropTable(
                name: "organization_limits");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "platform_role_assignments");

            migrationBuilder.DropTable(
                name: "provider_credentials");

            migrationBuilder.DropTable(
                name: "provider_health_checks");

            migrationBuilder.DropTable(
                name: "quota_usage");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "scheduled_notifications");

            migrationBuilder.DropTable(
                name: "webhook_events");

            migrationBuilder.DropTable(
                name: "campaigns");

            migrationBuilder.DropTable(
                name: "organization_memberships");

            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "notification_deliveries");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "sms_gateways");

            migrationBuilder.DropTable(
                name: "whatsapp_senders");

            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "notification_batches");

            migrationBuilder.DropTable(
                name: "template_versions");

            migrationBuilder.DropTable(
                name: "provider_instances");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropTable(
                name: "provider_accounts");

            migrationBuilder.DropTable(
                name: "applications");

            migrationBuilder.DropTable(
                name: "provider_definitions");

            migrationBuilder.DropTable(
                name: "organizations");
        }
    }
}
