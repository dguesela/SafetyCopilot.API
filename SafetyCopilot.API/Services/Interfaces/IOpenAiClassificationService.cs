using SafetyCopilot.API.DTOs;

namespace SafetyCopilot.API.Services;

public interface IOpenAiClassificationService
{
    Task<AiClassificationRunResponse>
        ClassifyProjectAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default);

    Task<AiClassificationResultsResponse?>
        GetProjectResultsAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default);
}