using Npgsql;

namespace NotificationService.Infrastructure.Tests;

[Collection(PostgreSqlCollection.CollectionName)]
public sealed class PersistenceSchemaIntegrationTests(PostgreSqlIntegrationFixture fixture)
{
    [Fact]
    public async Task Migrations_Should_CreateExpectedTables()
    {
        var expectedTables = new[]
        {
            "organizations", "applications", "users", "organization_memberships", "roles", "permissions",
            "membership_roles", "role_permissions", "platform_role_assignments", "refresh_tokens", "api_keys",
            "application_permissions", "application_limits", "organization_limits", "provider_definitions",
            "provider_accounts", "provider_credentials", "provider_instances", "whatsapp_senders", "application_senders",
            "sms_gateways", "provider_health_checks", "notification_batches", "notifications", "notification_deliveries",
            "notification_attempts", "delivery_status_history", "notification_status_history", "notification_events",
            "idempotency_records", "templates", "template_versions", "attachments", "notification_attachments",
            "campaigns", "campaign_recipients", "scheduled_notifications", "quota_usage", "webhook_events",
            "outbox_messages", "inbox_messages", "audit_logs"
        };

        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();

        foreach (var table in expectedTables)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*)
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = @tableName;
                """;
            command.Parameters.AddWithValue("tableName", table);

            var exists = (long)(await command.ExecuteScalarAsync() ?? 0L);
            Assert.Equal(1L, exists);
        }
    }

    [Fact]
    public async Task Schema_Should_EnforceUniqueSmsGatewayPerProviderAccount()
    {
        var organizationId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        var providerAccountId = Guid.NewGuid();

        await ExecuteAsync(
            "INSERT INTO organizations (\"Id\", \"Code\", \"Name\", \"Status\", \"Timezone\", \"CreatedAt\", \"UpdatedAt\") VALUES (@id, @code, @name, @status, @tz, now(), now());",
            new("id", organizationId), new("code", "org-sms"), new("name", "Org SMS"), new("status", "Active"), new("tz", "America/Lima"));

        await ExecuteAsync(
            "INSERT INTO provider_definitions (\"Id\", \"Code\", \"Channel\", \"AdapterKey\", \"Enabled\") VALUES (@id, @code, @channel, @adapter, true);",
            new("id", definitionId), new("code", "AndroidSmsGateway"), new("channel", "Sms"), new("adapter", "android-sms"));

        await ExecuteAsync(
            """
            INSERT INTO provider_accounts
                ("Id", "OrganizationId", "ProviderDefinitionId", "Code", "Name", "Status", "ConfigurationJson", "IsDefault", "Channel", "CreatedAt", "UpdatedAt")
            VALUES
                (@id, @org, @def, @code, @name, @status, '{}'::jsonb, false, @channel, now(), now());
            """,
            new("id", providerAccountId), new("org", organizationId), new("def", definitionId), new("code", "sms-acc"),
            new("name", "SMS Account"), new("status", "Active"), new("channel", "Sms"));

        await ExecuteAsync(
            """
            INSERT INTO sms_gateways ("Id", "OrganizationId", "ProviderAccountId", "DeviceLabel", "DeviceIdentifier", "SimSlot", "Status", "LastSeenAt")
            VALUES (@id, @org, @providerAccount, @label, @identifier, @slot, @status, now());
            """,
            new("id", Guid.NewGuid()), new("org", organizationId), new("providerAccount", providerAccountId),
            new("label", "Gateway A"), new("identifier", "dev-1"), new("slot", "SIM1"), new("status", "Active"));

        var exception = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            """
            INSERT INTO sms_gateways ("Id", "OrganizationId", "ProviderAccountId", "DeviceLabel", "DeviceIdentifier", "SimSlot", "Status", "LastSeenAt")
            VALUES (@id, @org, @providerAccount, @label, @identifier, @slot, @status, now());
            """,
            new("id", Guid.NewGuid()), new("org", organizationId), new("providerAccount", providerAccountId),
            new("label", "Gateway B"), new("identifier", "dev-2"), new("slot", "SIM2"), new("status", "Active")));

        Assert.Equal("23505", exception.SqlState);
    }

    [Fact]
    public async Task Schema_Should_EnforceSingleDefaultProviderAccountPerOrganizationAndChannel()
    {
        var organizationId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();

        await ExecuteAsync(
            "INSERT INTO organizations (\"Id\", \"Code\", \"Name\", \"Status\", \"Timezone\", \"CreatedAt\", \"UpdatedAt\") VALUES (@id, @code, @name, @status, @tz, now(), now());",
            new("id", organizationId), new("code", "org-provider"), new("name", "Org Provider"), new("status", "Active"), new("tz", "America/Lima"));

        await ExecuteAsync(
            "INSERT INTO provider_definitions (\"Id\", \"Code\", \"Channel\", \"AdapterKey\", \"Enabled\") VALUES (@id, @code, @channel, @adapter, true);",
            new("id", definitionId), new("code", "CustomSmtp"), new("channel", "Email"), new("adapter", "custom-smtp"));

        await ExecuteAsync(
            """
            INSERT INTO provider_accounts
                ("Id", "OrganizationId", "ProviderDefinitionId", "Code", "Name", "Status", "ConfigurationJson", "IsDefault", "Channel", "CreatedAt", "UpdatedAt")
            VALUES
                (@id, @org, @def, @code, @name, @status, '{}'::jsonb, true, @channel, now(), now());
            """,
            new("id", Guid.NewGuid()), new("org", organizationId), new("def", definitionId), new("code", "smtp-a"),
            new("name", "SMTP A"), new("status", "Active"), new("channel", "Email"));

        var exception = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            """
            INSERT INTO provider_accounts
                ("Id", "OrganizationId", "ProviderDefinitionId", "Code", "Name", "Status", "ConfigurationJson", "IsDefault", "Channel", "CreatedAt", "UpdatedAt")
            VALUES
                (@id, @org, @def, @code, @name, @status, '{}'::jsonb, true, @channel, now(), now());
            """,
            new("id", Guid.NewGuid()), new("org", organizationId), new("def", definitionId), new("code", "smtp-b"),
            new("name", "SMTP B"), new("status", "Active"), new("channel", "Email")));

        Assert.Equal("23505", exception.SqlState);
    }

    [Fact]
    public async Task Schema_Should_EnforceUniqueInboxMessageByConsumer()
    {
        var messageId = Guid.NewGuid().ToString("N");

        await ExecuteAsync(
            "INSERT INTO inbox_messages (\"Id\", \"ConsumerName\", \"MessageId\", \"ReceivedAt\", \"ProcessedAt\") VALUES (@id, @consumer, @message, now(), null);",
            new("id", Guid.NewGuid()), new("consumer", "worker-a"), new("message", messageId));

        var exception = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            "INSERT INTO inbox_messages (\"Id\", \"ConsumerName\", \"MessageId\", \"ReceivedAt\", \"ProcessedAt\") VALUES (@id, @consumer, @message, now(), null);",
            new("id", Guid.NewGuid()), new("consumer", "worker-a"), new("message", messageId)));

        Assert.Equal("23505", exception.SqlState);
    }

    [Fact]
    public async Task Schema_Should_EnforceIdempotencyUniquenessPerApplication()
    {
        var organizationId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        const string key = "idem-123";

        await ExecuteAsync(
            "INSERT INTO organizations (\"Id\", \"Code\", \"Name\", \"Status\", \"Timezone\", \"CreatedAt\", \"UpdatedAt\") VALUES (@id, @code, @name, @status, @tz, now(), now());",
            new("id", organizationId), new("code", "org-idem"), new("name", "Org Idem"), new("status", "Active"), new("tz", "America/Lima"));

        await ExecuteAsync(
            "INSERT INTO applications (\"Id\", \"OrganizationId\", \"Code\", \"Name\", \"Status\", \"CreatedAt\", \"UpdatedAt\") VALUES (@id, @org, @code, @name, @status, now(), now());",
            new("id", applicationId), new("org", organizationId), new("code", "app-idem"), new("name", "App Idem"), new("status", "Active"));

        await ExecuteAsync(
            """
            INSERT INTO idempotency_records ("Id", "OrganizationId", "ApplicationId", "IdempotencyKey", "RequestHash", "ResourceId", "ResponseStatus", "ExpiresAt")
            VALUES (@id, @org, @app, @key, @hash, @resource, @status, now() + interval '10 minutes');
            """,
            new("id", Guid.NewGuid()), new("org", organizationId), new("app", applicationId), new("key", key),
            new("hash", "hash-1"), new("resource", Guid.NewGuid()), new("status", 202));

        var exception = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            """
            INSERT INTO idempotency_records ("Id", "OrganizationId", "ApplicationId", "IdempotencyKey", "RequestHash", "ResourceId", "ResponseStatus", "ExpiresAt")
            VALUES (@id, @org, @app, @key, @hash, @resource, @status, now() + interval '10 minutes');
            """,
            new("id", Guid.NewGuid()), new("org", organizationId), new("app", applicationId), new("key", key),
            new("hash", "hash-2"), new("resource", Guid.NewGuid()), new("status", 202)));

        Assert.Equal("23505", exception.SqlState);
    }

    private async Task ExecuteAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }

        await command.ExecuteNonQueryAsync();
    }
}
