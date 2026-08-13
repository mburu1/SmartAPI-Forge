using SmartAPIForge.Domain.Common;

namespace SmartAPIForge.Infrastructure.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Postgres;
}
