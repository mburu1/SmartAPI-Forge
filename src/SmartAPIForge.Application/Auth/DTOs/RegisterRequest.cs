using System.ComponentModel.DataAnnotations;

namespace SmartAPIForge.Application.Auth.DTOs;

public record RegisterRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public required string Password { get; init; }

    [MaxLength(128)]
    public string? DisplayName { get; init; }
}
