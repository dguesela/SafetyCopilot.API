using SafetyCopilot.API.DTOs;

namespace SafetyCopilot.API.Services;

public interface IOpenAiClassificationService
{
    Task<RevisionDecisionResponse?> SaveRevisionDecisionAsync(
        Guid projectId, Guid requirementId, RevisionDecisionRequest request,
        CancellationToken cancellationToken = default);

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