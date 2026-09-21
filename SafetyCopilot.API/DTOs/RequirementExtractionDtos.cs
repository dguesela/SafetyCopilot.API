namespace SafetyCopilot.API.DTOs
{

    public record RequirementExtractionResponse(
        Guid DocumentId,
        string FileName,
        int ExtractedRequirementCount
    );
}