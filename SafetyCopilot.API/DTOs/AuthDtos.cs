using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.DTOs
{

    public record RegisterRequest(
        [Required]
    [EmailAddress]
    string Email,

        [Required]
    [MinLength(8)]
    string Password,

        [MaxLength(200)]
    string? DisplayName
    );

    public record RegisterResponse(
        Guid Id,
        string Email,
        string? DisplayName
    );

    public record LoginRequest(
        [Required]
    [EmailAddress]
    string Email,

        [Required]
    string Password
    );

    public record LoginResponse(
        Guid Id,
        string Email,
        string? DisplayName
    );
}