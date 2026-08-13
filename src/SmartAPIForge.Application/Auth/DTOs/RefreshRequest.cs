using System.ComponentModel.DataAnnotations;

namespace SmartAPIForge.Application.Auth.DTOs;

public record RefreshRequest
{
    [Required]
    public required string RefreshToken { get; init; }
}
