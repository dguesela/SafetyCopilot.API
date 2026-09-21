using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.DTOs;

public record RunAiClassificationRequest(
    [Required]
    Guid UserId
);

public record AiClassificationResponse(
    Guid RequirementId,
    bool IsHazard,
    double? Confidence,
    string? Explanation,
    string? ModelName,
    DateTime ClassifiedAtUtc
);

public record AiClassificationRunResponse(
    Guid ProjectId,
    int TotalRequirements,
    int ClassifiedRequirements,
    string ModelName
);

public record AiClassificationDetailsResponse(
    Guid RequirementId,
    string? RequirementNumber,
    string RequirementText,
    bool IsHazard,
    double? Confidence,
    string? Explanation,
    string? ModelName,
    DateTime ClassifiedAtUtc
);

public record AiClassificationResultsResponse(
    Guid ProjectId,
    int TotalRequirements,
    int HazardCount,
    int NotHazardCount,
    double? AverageConfidence,
    string? ModelName,
    IReadOnlyList<AiClassificationDetailsResponse> Results
);