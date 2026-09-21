using Microsoft.AspNetCore.Http;
using SafetyCopilot.API.DTOs;

namespace SafetyCopilot.API.Services.Interfaces
{

    public interface IRequirementDocumentService
    {
        Task<IReadOnlyList<RequirementDocumentResponse>>
            GetProjectDocumentsAsync(
                Guid projectId,
                CancellationToken cancellationToken = default);

        Task<RequirementDocumentDetailsResponse?>
            GetDocumentAsync(
                Guid documentId,
                CancellationToken cancellationToken = default);

        Task<RequirementDocumentResponse>
            UploadAsync(
                Guid projectId,
                IFormFile file,
                CancellationToken cancellationToken = default);

        Task<RequirementExtractionResponse>
            ExtractRequirementsAsync(
                Guid documentId,
                CancellationToken cancellationToken = default);

        Task<bool>
            DeleteAsync(
                Guid documentId,
                CancellationToken cancellationToken = default);
    }
}