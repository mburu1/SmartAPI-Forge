using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SmartAPIForge.Infrastructure.Options;

namespace SmartAPIForge.Api.Controllers;

public record SystemStatusResponse(
    string Status,
    string Environment,
    DateTime ServerTimeUtc,
    string DatabaseProvider,
    TimeSpan Uptime,
    string Version);

/// <summary>Operational status for the dashboard — not a business endpoint, so it's unauthenticated by design.</summary>
[ApiController]
[Route("system")]
public class SystemController(
    IOptions<DatabaseOptions> databaseOptions,
    IHostEnvironment environment,
    HealthCheckService healthCheckService) : ControllerBase
{
    private static readonly DateTime ProcessStartUtc = Process.GetCurrentProcess().StartTime.ToUniversalTime();

    [HttpGet("status")]
    [ProducesResponseType<SystemStatusResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);
        var version = typeof(SystemController).Assembly.GetName().Version?.ToString() ?? "unknown";

        return Ok(new SystemStatusResponse(
            Status: report.Status.ToString(),
            Environment: environment.EnvironmentName,
            ServerTimeUtc: DateTime.UtcNow,
            DatabaseProvider: databaseOptions.Value.Provider.ToString(),
            Uptime: DateTime.UtcNow - ProcessStartUtc,
            Version: version));
    }
}
