using System.ComponentModel.DataAnnotations;

namespace NotificationService.Infrastructure.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    [MinLength(1)]
    public string ConnectionString { get; init; } = string.Empty;
}