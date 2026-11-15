using Microsoft.Extensions.Options;
using Npgsql;

namespace NotificationService.Infrastructure.Configuration;

/// <summary>
/// Valida la cadena de conexión al iniciar el host. Los mensajes nunca incluyen el valor
/// de la cadena para no filtrar credenciales en logs.
/// </summary>
internal sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail(
                $"{DatabaseOptions.ConfigurationKey} is required. Configure it with User Secrets or environment variables.");
        }

        NpgsqlConnectionStringBuilder builder;
        try
        {
            builder = new NpgsqlConnectionStringBuilder(options.ConnectionString);
        }
        catch (ArgumentException)
        {
            return ValidateOptionsResult.Fail(
                $"{DatabaseOptions.ConfigurationKey} is not a valid PostgreSQL connection string.");
        }

        if (string.IsNullOrWhiteSpace(builder.Host))
        {
            return ValidateOptionsResult.Fail($"{DatabaseOptions.ConfigurationKey} must define Host.");
        }

        if (string.IsNullOrWhiteSpace(builder.Database))
        {
            return ValidateOptionsResult.Fail($"{DatabaseOptions.ConfigurationKey} must define Database.");
        }

        return ValidateOptionsResult.Success;
    }
}
