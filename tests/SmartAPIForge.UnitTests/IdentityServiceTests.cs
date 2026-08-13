using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using SmartAPIForge.Application.Auth.DTOs;
using SmartAPIForge.Infrastructure.Identity;
using SmartAPIForge.Infrastructure.Options;
using SmartAPIForge.Infrastructure.Persistence;
using Xunit;

namespace SmartAPIForge.UnitTests;

public class IdentityServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static JwtTokenGenerator CreateJwtGenerator() => new(Options.Create(new JwtOptions
    {
        Key = Convert.ToBase64String(new byte[32]),
        Issuer = "SmartAPIForge.Tests",
        Audience = "SmartAPIForge.Tests.Clients"
    }));

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        var userManager = CreateUserManagerMock();
        var existingUser = new ApplicationUser { Email = "taken@example.com" };
        userManager.Setup(m => m.FindByEmailAsync("taken@example.com")).ReturnsAsync(existingUser);

        var sut = new IdentityService(userManager.Object, db, CreateJwtGenerator(), Options.Create(new JwtOptions
        {
            Key = Convert.ToBase64String(new byte[32]),
            Issuer = "i",
            Audience = "a"
        }));

        var result = await sut.RegisterAsync(new RegisterRequest { Email = "taken@example.com", Password = "Password1!" });

        Assert.False(result.Succeeded);
        Assert.Contains("already exists", result.Errors.Single());
    }

    [Fact]
    public async Task RegisterAsync_WhenNewUser_IssuesTokensAndPersistsRefreshToken()
    {
        await using var db = CreateDbContext();
        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync("new@example.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Password1!"))
            .Callback<ApplicationUser, string>((u, _) => u.Id = Guid.NewGuid())
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync([]);

        var jwtOptions = Options.Create(new JwtOptions { Key = Convert.ToBase64String(new byte[32]), Issuer = "i", Audience = "a" });
        var sut = new IdentityService(userManager.Object, db, CreateJwtGenerator(), jwtOptions);

        var result = await sut.RegisterAsync(new RegisterRequest { Email = "new@example.com", Password = "Password1!" });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Single(db.RefreshTokens);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIncorrect_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        var userManager = CreateUserManagerMock();
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@example.com" };
        userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

        var jwtOptions = Options.Create(new JwtOptions { Key = Convert.ToBase64String(new byte[32]), Issuer = "i", Audience = "a" });
        var sut = new IdentityService(userManager.Object, db, CreateJwtGenerator(), jwtOptions);

        var result = await sut.LoginAsync(new LoginRequest { Email = "user@example.com", Password = "wrong" });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenActive_RotatesAndRevokesOldToken()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "user@example.com" };
        var oldToken = new Domain.Entities.RefreshToken
        {
            Token = "old-token",
            UserId = userId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
        };
        db.RefreshTokens.Add(oldToken);
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);

        var jwtOptions = Options.Create(new JwtOptions { Key = Convert.ToBase64String(new byte[32]), Issuer = "i", Audience = "a" });
        var sut = new IdentityService(userManager.Object, db, CreateJwtGenerator(), jwtOptions);

        var result = await sut.RefreshAsync(new RefreshRequest { RefreshToken = "old-token" });

        Assert.True(result.Succeeded);
        Assert.NotEqual("old-token", result.RefreshToken);

        var reloaded = await db.RefreshTokens.FirstAsync(rt => rt.Token == "old-token");
        Assert.False(reloaded.IsActive);
        Assert.Equal(result.RefreshToken, reloaded.ReplacedByToken);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenUnknown_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        var userManager = CreateUserManagerMock();
        var jwtOptions = Options.Create(new JwtOptions { Key = Convert.ToBase64String(new byte[32]), Issuer = "i", Audience = "a" });
        var sut = new IdentityService(userManager.Object, db, CreateJwtGenerator(), jwtOptions);

        var result = await sut.RefreshAsync(new RefreshRequest { RefreshToken = "does-not-exist" });

        Assert.False(result.Succeeded);
    }
}
