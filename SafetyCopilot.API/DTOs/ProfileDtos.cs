using System.ComponentModel.DataAnnotations;


namespace SafetyCopilot.API.DTOs
{
    
    
    public record ProfileResponse(
        Guid Id,
        string Email,
        string? DisplayName,
        bool HasOpenAiApiKey
    );

    public record UpdateProfileRequest(
        [MaxLength(200)]
    string? DisplayName
    );

    public record SaveOpenAiApiKeyRequest(
        [Required]
    string ApiKey
    );

    public record ApiKeyStatusResponse(
        bool IsConfigured
    );
}
