using SmartAPIForge.Domain.Common;

namespace SmartAPIForge.Domain.Entities;

/// <summary>
/// A rotatable refresh token issued to a user, stored server-side so it can be
/// revoked/rotated independently of the short-lived JWT access token.
/// </summary>
public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }

    public required Guid UserId { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
