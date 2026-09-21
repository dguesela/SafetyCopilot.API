using SafetyCopilot.API.DTOs;

namespace SafetyCopilot.API.Services.Interfaces
{
    public interface IExperimentService
    {
        Task<ExperimentSessionResponse> CreateAsync(
            Guid userId,
            CreateExperimentSessionRequest request);

        Task<ExperimentSessionResponse?> CompleteAsync(
            Guid userId,
            Guid sessionId);

        Task RecordMeasurementAsync(
            Guid userId,
            Guid sessionId,
            RecordMeasurementRequest request);
    }
}
