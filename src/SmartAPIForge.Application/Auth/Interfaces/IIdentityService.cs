using SmartAPIForge.Application.Auth.DTOs;

namespace SmartAPIForge.Application.Auth.Interfaces;

public interface IIdentityService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
