using SafetyCopilot.API.DTOs;

namespace SafetyCopilot.API.Services.Interfaces
{
    public interface IAnalysisService
    {
        Task<AnalysisSummaryResponse>
            GetProjectAnalysisAsync(
                Guid projectId,
                CancellationToken cancellationToken = default);
    }
}
