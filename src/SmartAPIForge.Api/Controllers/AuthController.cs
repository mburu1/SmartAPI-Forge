using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAPIForge.Application.Auth.DTOs;
using SmartAPIForge.Application.Auth.Interfaces;

namespace SmartAPIForge.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IIdentityService identityService) : ControllerBase
{
    /// <summary>Creates a new user account and returns an access/refresh token pair.</summary>
    [HttpPost("register")]
    [ProducesResponseType<AuthResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    /// <summary>Exchanges valid credentials for an access/refresh token pair.</summary>
    [HttpPost("login")]
    [ProducesResponseType<AuthResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.LoginAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Exchanges a valid, unexpired refresh token for a new access/refresh token pair.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await identityService.RefreshAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Returns the authenticated user's profile. Verifies the bearer token is valid.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (userId is null || !Guid.TryParse(userId, out var id))
        {
            return Unauthorized();
        }

        var user = await identityService.GetUserAsync(id, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}
