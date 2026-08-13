using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SmartAPIForge.IntegrationTests;

/// <summary>
/// Boots the real Api host but points it at EF Core's in-memory provider
/// (a unique database per factory instance) instead of a real
/// Postgres/SqlServer/MySql instance. Uses UseSetting rather than
/// ConfigureServices/RemoveAll because EF Core registers a relational
/// provider's internal services into the container the moment
/// AddDbContext's options action runs (during Program.cs's own startup,
/// before this factory gets a chance to intervene) — swapping providers
/// after the fact leaves both registered and EF Core refuses to start.
/// Steering Program.cs's own provider selection via configuration avoids
/// that entirely.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"IntegrationTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("Database:Provider", "InMemory");
        builder.UseSetting("ConnectionStrings:InMemory", _databaseName);
        builder.UseSetting("Jwt:Key", Convert.ToBase64String(new byte[32]));
        builder.UseSetting("Jwt:Issuer", "SmartAPIForge.IntegrationTests");
        builder.UseSetting("Jwt:Audience", "SmartAPIForge.IntegrationTests.Clients");
    }
}
