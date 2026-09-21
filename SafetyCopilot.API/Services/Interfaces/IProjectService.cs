using SafetyCopilot.API.DTOs;

namespace SafetyCopilot.API.Services.Interfaces
{

    public interface IProjectService
    {
        Task<IReadOnlyList<ProjectResponse>>
            GetByUserAsync(
                Guid userId,
                CancellationToken cancellationToken = default);

        Task<ProjectResponse?>
            GetByIdAsync(
                Guid projectId,
                CancellationToken cancellationToken = default);

        Task<ProjectResponse>
            CreateAsync(
                CreateProjectRequest request,
                CancellationToken cancellationToken = default);

        Task<ProjectResponse?>
            UpdateAsync(
                Guid projectId,
                UpdateProjectRequest request,
                CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);
    }
}