namespace SmartAPIForge.Domain.Common;

/// <summary>
/// Relational database providers the API can be configured against via
/// the "Database:Provider" setting. Selecting a provider determines which
/// ConnectionStrings entry and EF Core provider get wired up at startup.
/// </summary>
public enum DatabaseProvider
{
    Postgres,
    SqlServer,
    MySql,

    /// <summary>
    /// EF Core's in-memory provider. Only ever selected by the integration
    /// test host (via the "InMemory" ConnectionStrings entry) — never a
    /// valid choice for a real deployment.
    /// </summary>
    InMemory
}
