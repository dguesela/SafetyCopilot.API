namespace SafetyCopilot.API.DTOs
{

    public record RequirementComparisonResponse(
        Guid RequirementId,
        Guid DocumentId,
        string? RequirementNumber,
        string RequirementText,

        bool HumanIsHazard,
        bool AiIsHazard,

        bool IsAgreement,

        string ComparisonCategory,

        double? AiConfidence,
        string? AiExplanation,
        string? AiModelName
    );

    public record AnalysisSummaryResponse(
        Guid ProjectId,

        int TotalRequirements,

        int AgreementCount,

        int HazardAgreementCount,

        int NonHazardAgreementCount,

        int HumanOnlyHazardCount,

        int AiOnlyHazardCount,

        double AgreementPercentage,

        IReadOnlyList<RequirementComparisonResponse>
            Agreements,

        IReadOnlyList<RequirementComparisonResponse>
            HumanOnlyHazards,

        IReadOnlyList<RequirementComparisonResponse>
            AiOnlyHazards
    );
}