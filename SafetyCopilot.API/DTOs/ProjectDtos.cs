using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.DTOs
{

    public record CreateProjectRequest(
        [Required]
    Guid UserId,

        [Required]
    [MaxLength(200)]
    string Name,

        [MaxLength(2000)]
    string? Description
    );

    public record UpdateProjectRequest(
        [Required]
    [MaxLength(200)]
    string Name,

        [MaxLength(2000)]
    string? Description
    );

    public record ProjectResponse(
        Guid Id,
        Guid UserId,
        string Name,
        string? Description,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc,
        int DocumentCount,
        int RequirementCount
    );
}