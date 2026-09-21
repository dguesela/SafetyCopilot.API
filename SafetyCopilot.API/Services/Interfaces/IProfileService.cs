using SafetyCopilot.API.DTOs;

namespace SafetyCopilot.API.Services.Interfaces
{

    public interface IProfileService
    {
        Task<ProfileResponse?> GetAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<ProfileResponse?> UpdateAsync(
            Guid userId,
            UpdateProfileRequest request,
            CancellationToken cancellationToken = default);

        Task<bool> SaveApiKeyAsync(
            Guid userId,
            string apiKey,
            CancellationToken cancellationToken = default);

        Task<bool> RemoveApiKeyAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<string?> GetDecryptedApiKeyAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}