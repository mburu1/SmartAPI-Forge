using Microsoft.AspNetCore.Identity;

namespace SmartAPIForge.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user. Kept in Infrastructure (not Domain) so the
/// Domain and Application layers stay free of framework dependencies.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
