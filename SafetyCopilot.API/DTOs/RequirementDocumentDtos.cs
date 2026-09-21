namespace SafetyCopilot.API.DTOs
{
    public record RequirementDocumentResponse(
        Guid Id,
        Guid ProjectId,
        string FileName,
        string ContentType,
        long FileSizeBytes,
        DateTime UploadedAtUtc,
        DateTime? ProcessedAtUtc,
        int RequirementCount
    );

    public record RequirementDocumentDetailsResponse(
        Guid Id,
        Guid ProjectId,
        string FileName,
        string ContentType,
        long FileSizeBytes,
        string? ExtractedText,
        DateTime UploadedAtUtc,
        DateTime? ProcessedAtUtc,
        int RequirementCount
    );
}
