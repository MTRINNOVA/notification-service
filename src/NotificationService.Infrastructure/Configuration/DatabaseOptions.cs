namespace NotificationService.Infrastructure.Configuration;

/// <summary>
/// Opciones de conexión a PostgreSQL. Se leen de <c>ConnectionStrings:DefaultConnection</c>
/// (variable de entorno <c>ConnectionStrings__DefaultConnection</c>). Es la única convención admitida.
/// </summary>
public sealed class DatabaseOptions
{
    public const string ConnectionStringName = "DefaultConnection";

    public const string ConfigurationKey = "ConnectionStrings:" + ConnectionStringName;

    public string ConnectionString { get; set; } = string.Empty;
}
