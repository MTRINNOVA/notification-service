using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using NotificationService.Api.Common;

namespace NotificationService.Api.Tests;

/// <summary>
/// Host de pruebas de la API con configuración en memoria (prioridad sobre appsettings, User Secrets y variables de entorno).
/// </summary>
internal sealed class ApiTestFactory(string environment, IDictionary<string, string?>? overrides = null)
    : WebApplicationFactory<ApiAssemblyMarker>
{
    public const string ConnectionStringKey = "ConnectionStrings:DefaultConnection";

    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=notification_test;Username=test;Password=placeholder";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var settings = new Dictionary<string, string?>
        {
            [ConnectionStringKey] = TestConnectionString,
            ["OpenApi:Enabled"] = "false"
        };

        foreach (var (key, value) in overrides ?? new Dictionary<string, string?>())
        {
            settings[key] = value;
        }

        builder.UseEnvironment(environment);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings));
    }
}
