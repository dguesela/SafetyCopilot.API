namespace SafetyCopilot.API.DTOs
{
    public record CreateExperimentSessionRequest(
     Guid ProjectId,
     string Condition);

    public record RecordMeasurementRequest(
        string MeasurementType,
        decimal Value,
        string? Unit);

    public record ExperimentSessionResponse(
        Guid Id,
        Guid ProjectId,
        string Condition,
        string Status,
        DateTime StartedAtUtc,
        DateTime? CompletedAtUtc);
}
