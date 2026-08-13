namespace SmartAPIForge.Application.Auth.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>Generates a short-lived signed JWT access token for the given user.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);

    /// <summary>Generates a cryptographically random opaque refresh token.</summary>
    string GenerateRefreshToken();
}
