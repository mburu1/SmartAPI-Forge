using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartAPIForge.Application.Auth.DTOs;
using SmartAPIForge.Application.Auth.Interfaces;
using SmartAPIForge.Domain.Entities;
using SmartAPIForge.Infrastructure.Options;
using SmartAPIForge.Infrastructure.Persistence;

namespace SmartAPIForge.Infrastructure.Identity;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator,
    IOptions<JwtOptions> jwtOptions) : IIdentityService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return AuthResult.Failure("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return AuthResult.Failure(createResult.Errors.Select(e => e.Description));
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return AuthResult.Failure("Invalid email or password.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return AuthResult.Failure("Invalid or expired refresh token.");
        }

        var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null)
        {
            return AuthResult.Failure("Invalid or expired refresh token.");
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;

        var result = await IssueTokensAsync(user, cancellationToken);
        storedToken.ReplacedByToken = result.RefreshToken;
        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : ToUserDto(user);
    }

    private async Task<AuthResult> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, accessTokenExpiresAtUtc) = jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshTokenValue = jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAtUtc = refreshTokenExpiresAtUtc
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(accessToken, accessTokenExpiresAtUtc, refreshTokenValue, refreshTokenExpiresAtUtc, ToUserDto(user));
    }

    private static UserDto ToUserDto(ApplicationUser user) => new(user.Id, user.Email!, user.DisplayName);
}
