using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using SmartAPIForge.Infrastructure.Identity;
using SmartAPIForge.Infrastructure.Options;
using Xunit;

namespace SmartAPIForge.UnitTests;

public class JwtTokenGeneratorTests
{
    private static JwtTokenGenerator CreateGenerator(int accessTokenMinutes = 15) =>
        new(Options.Create(new JwtOptions
        {
            Key = Convert.ToBase64String(new byte[32]),
            Issuer = "SmartAPIForge.Tests",
            Audience = "SmartAPIForge.Tests.Clients",
            AccessTokenMinutes = accessTokenMinutes
        }));

    [Fact]
    public void GenerateAccessToken_ProducesTokenWithExpectedClaimsAndIssuer()
    {
        var generator = CreateGenerator();
        var userId = Guid.NewGuid();

        var (token, expiresAtUtc) = generator.GenerateAccessToken(userId, "user@example.com", ["Admin"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("SmartAPIForge.Tests", jwt.Issuer);
        Assert.Contains(jwt.Audiences, a => a == "SmartAPIForge.Tests.Clients");
        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("user@example.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
        Assert.True(expiresAtUtc > DateTime.UtcNow.AddMinutes(14) && expiresAtUtc <= DateTime.UtcNow.AddMinutes(15));
    }

    [Fact]
    public void GenerateRefreshToken_ProducesUniqueValues()
    {
        var generator = CreateGenerator();

        var first = generator.GenerateRefreshToken();
        var second = generator.GenerateRefreshToken();

        Assert.NotEqual(first, second);
        Assert.True(Convert.FromBase64String(first).Length >= 64);
    }
}
