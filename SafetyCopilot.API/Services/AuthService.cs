using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Models;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Services
{

    public class AuthService : IAuthService
    {
        private readonly SafetyDbContext _db;

        public AuthService(
            SafetyDbContext db)
        {
            _db = db;
        }

        public async Task<RegisterResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            var email = request.Email
                .Trim()
                .ToLowerInvariant();

            var existingUser =
                await _db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Email.ToLower() == email,
                        cancellationToken);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "A user with this email address already exists.");
            }

            var passwordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = passwordHash,
                DisplayName =
                    string.IsNullOrWhiteSpace(
                        request.DisplayName)
                        ? null
                        : request.DisplayName.Trim(),
                OpenAiApiKeyEncrypted = null,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = null
            };

            _db.Users.Add(user);

            await _db.SaveChangesAsync(
                cancellationToken);

            return new RegisterResponse(
                user.Id,
                user.Email,
                user.DisplayName);
        }

        public async Task<LoginResponse?> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            var email = request.Email
                .Trim()
                .ToLowerInvariant();

            var user =
                await _db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Email.ToLower() == email,
                        cancellationToken);

            if (user == null)
            {
                return null;
            }

            bool passwordValid;

            try
            {
                passwordValid =
                    BCrypt.Net.BCrypt.Verify(
                        request.Password,
                        user.PasswordHash);
            }
            catch
            {
                return null;
            }

            if (!passwordValid)
            {
                return null;
            }

            return new LoginResponse(
                user.Id,
                user.Email,
                user.DisplayName);
        }
    }
}