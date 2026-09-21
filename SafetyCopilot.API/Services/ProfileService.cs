using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Services
{

    public class ProfileService : IProfileService
    {
        private readonly SafetyDbContext _db;

        private readonly ISecretEncryptionService
            _secretEncryptionService;

        public ProfileService(
            SafetyDbContext db,
            ISecretEncryptionService
                secretEncryptionService)
        {
            _db = db;

            _secretEncryptionService =
                secretEncryptionService;
        }

        public async Task<ProfileResponse?> GetAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x =>
                    new ProfileResponse(
                        x.Id,
                        x.Email,
                        x.DisplayName,
                        x.OpenAiApiKeyEncrypted != null))
                .FirstOrDefaultAsync(
                    cancellationToken);
        }

        public async Task<ProfileResponse?> UpdateAsync(
            Guid userId,
            UpdateProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            var user =
                await _db.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == userId,
                        cancellationToken);

            if (user == null)
            {
                return null;
            }

            user.DisplayName =
                string.IsNullOrWhiteSpace(
                    request.DisplayName)
                    ? null
                    : request.DisplayName.Trim();

            user.UpdatedAtUtc =
                DateTime.UtcNow;

            await _db.SaveChangesAsync(
                cancellationToken);

            return new ProfileResponse(
                user.Id,
                user.Email,
                user.DisplayName,
                !string.IsNullOrWhiteSpace(
                    user.OpenAiApiKeyEncrypted));
        }

        public async Task<bool> SaveApiKeyAsync(
            Guid userId,
            string apiKey,
            CancellationToken cancellationToken = default)
        {
            var user =
                await _db.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == userId,
                        cancellationToken);

            if (user == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    apiKey))
            {
                throw new ArgumentException(
                    "OpenAI API key is required.");
            }

            user.OpenAiApiKeyEncrypted =
                _secretEncryptionService
                    .Encrypt(apiKey);

            user.UpdatedAtUtc =
                DateTime.UtcNow;

            await _db.SaveChangesAsync(
                cancellationToken);

            return true;
        }

        public async Task<bool> RemoveApiKeyAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user =
                await _db.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == userId,
                        cancellationToken);

            if (user == null)
            {
                return false;
            }

            user.OpenAiApiKeyEncrypted = null;

            user.UpdatedAtUtc =
                DateTime.UtcNow;

            await _db.SaveChangesAsync(
                cancellationToken);

            return true;
        }

        public async Task<string?>
            GetDecryptedApiKeyAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
        {
            var encryptedKey =
                await _db.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId)
                    .Select(
                        x =>
                            x.OpenAiApiKeyEncrypted)
                    .FirstOrDefaultAsync(
                        cancellationToken);

            if (string.IsNullOrWhiteSpace(
                    encryptedKey))
            {
                return null;
            }

            return _secretEncryptionService
                .Decrypt(encryptedKey);
        }
    }
}