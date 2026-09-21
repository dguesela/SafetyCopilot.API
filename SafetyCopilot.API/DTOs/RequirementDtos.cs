using System.ComponentModel.DataAnnotations;


namespace SafetyCopilot.API.DTOs
{
    
   
    public record RequirementResponse(
        Guid Id,
        Guid DocumentId,
        string? RequirementNumber,
        string RequirementText,
        int SequenceNumber,
        bool? HumanIsHazard,
        DateTime? HumanClassifiedAtUtc,
        bool HasAiClassification
    );

    public record HumanClassificationRequest(
        [Required]
    Guid UserId,

        [Required]
    bool IsHazard
    );

    public record HumanClassificationResponse(
        Guid RequirementId,
        Guid UserId,
        bool IsHazard,
        DateTime ClassifiedAtUtc
    );

    public record ClassificationProgressResponse(
        int TotalRequirements,
        int ClassifiedRequirements,
        int RemainingRequirements,
        bool IsComplete,
        double PercentComplete
    );
}
