namespace SmartAPIForge.Application.Auth.DTOs;

public record AuthResult
{
    public bool Succeeded { get; init; }

    public string? AccessToken { get; init; }

    public DateTime? AccessTokenExpiresAtUtc { get; init; }

    public string? RefreshToken { get; init; }

    public DateTime? RefreshTokenExpiresAtUtc { get; init; }

    public UserDto? User { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = [];

    public static AuthResult Success(string accessToken, DateTime accessTokenExpiresAtUtc,
        string refreshToken, DateTime refreshTokenExpiresAtUtc, UserDto user) => new()
        {
            Succeeded = true,
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc,
            User = user
        };

    public static AuthResult Failure(params IEnumerable<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors.ToArray()
    };
}
