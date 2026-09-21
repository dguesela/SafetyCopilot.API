using SafetyCopilot.API.DTOs;

namespace SafetyCopilot.API.Services.Interfaces;

public interface IRequirementService
{
    Task<IReadOnlyList<RequirementResponse>>
        GetProjectRequirementsAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RequirementResponse>>
        GetDocumentRequirementsAsync(
            Guid documentId,
            CancellationToken cancellationToken = default);

    Task<RequirementResponse?>
        GetRequirementAsync(
            Guid requirementId,
            CancellationToken cancellationToken = default);

    Task<HumanClassificationResponse>
        ClassifyAsync(
            Guid requirementId,
            HumanClassificationRequest request,
            CancellationToken cancellationToken = default);

    Task<ClassificationProgressResponse>
        GetProgressAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default);
}