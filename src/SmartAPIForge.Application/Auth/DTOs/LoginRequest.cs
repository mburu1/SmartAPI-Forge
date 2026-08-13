using System.ComponentModel.DataAnnotations;

namespace SmartAPIForge.Application.Auth.DTOs;

public record LoginRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
