using SmartAPIForge.Domain.Entities;
using Xunit;

namespace SmartAPIForge.UnitTests;

public class RefreshTokenTests
{
    private static RefreshToken CreateToken() => new()
    {
        Token = "token-value",
        UserId = Guid.NewGuid(),
        ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
    };

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ReturnsTrue()
    {
        var token = CreateToken();

        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenRevoked_ReturnsFalse()
    {
        var token = CreateToken();
        token.RevokedAtUtc = DateTime.UtcNow;

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        var token = CreateToken();
        token.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-1);

        Assert.False(token.IsActive);
    }
}
