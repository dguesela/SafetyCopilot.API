using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Models;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Services
{

    public class ProjectService
        : IProjectService
    {
        private readonly SafetyDbContext _db;

        public ProjectService(
            SafetyDbContext db)
        {
            _db = db;
        }

        public async Task<
            IReadOnlyList<ProjectResponse>>
            GetByUserAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
        {
            return await _db.Projects
                .AsNoTracking()
                .Where(
                    x => x.UserId == userId)
                .OrderByDescending(
                    x => x.CreatedAtUtc)
                .Select(
                    x =>
                        new ProjectResponse(
                            x.Id,
                            x.UserId,
                            x.Name,
                            x.Description,
                            x.CreatedAtUtc,
                            x.UpdatedAtUtc,

                            x.RequirementDocuments
                                .Count,

                            x.RequirementDocuments
                                .SelectMany(
                                    d => d.Requirements)
                                .Count()))
                .ToListAsync(
                    cancellationToken);
        }

        public async Task<ProjectResponse?>
            GetByIdAsync(
                Guid projectId,
                CancellationToken cancellationToken = default)
        {
            return await _db.Projects
                .AsNoTracking()
                .Where(
                    x => x.Id == projectId)
                .Select(
                    x =>
                        new ProjectResponse(
                            x.Id,
                            x.UserId,
                            x.Name,
                            x.Description,
                            x.CreatedAtUtc,
                            x.UpdatedAtUtc,

                            x.RequirementDocuments
                                .Count,

                            x.RequirementDocuments
                                .SelectMany(
                                    d => d.Requirements)
                                .Count()))
                .FirstOrDefaultAsync(
                    cancellationToken);
        }

        public async Task<ProjectResponse>
            CreateAsync(
                CreateProjectRequest request,
                CancellationToken cancellationToken = default)
        {
            var userExists =
                await _db.Users.AnyAsync(
                    x => x.Id == request.UserId,
                    cancellationToken);

            if (!userExists)
            {
                throw new InvalidOperationException(
                    "User does not exist.");
            }

            var project =
                new Project
                {
                    UserId =
                        request.UserId,

                    Name =
                        request.Name.Trim(),

                    Description =
                        string.IsNullOrWhiteSpace(
                            request.Description)
                            ? null
                            : request.Description.Trim(),

                    CreatedAtUtc =
                        DateTime.UtcNow
                };

            _db.Projects.Add(project);

            await _db.SaveChangesAsync(
                cancellationToken);

            return new ProjectResponse(
                project.Id,
                project.UserId,
                project.Name,
                project.Description,
                project.CreatedAtUtc,
                project.UpdatedAtUtc,
                0,
                0);
        }

        public async Task<ProjectResponse?>
            UpdateAsync(
                Guid projectId,
                UpdateProjectRequest request,
                CancellationToken cancellationToken = default)
        {
            var project =
                await _db.Projects
                    .FirstOrDefaultAsync(
                        x => x.Id == projectId,
                        cancellationToken);

            if (project == null)
            {
                return null;
            }

            project.Name =
                request.Name.Trim();

            project.Description =
                string.IsNullOrWhiteSpace(
                    request.Description)
                    ? null
                    : request.Description.Trim();

            project.UpdatedAtUtc =
                DateTime.UtcNow;

            await _db.SaveChangesAsync(
                cancellationToken);

            return await GetByIdAsync(
                projectId,
                cancellationToken);
        }

        public async Task<bool> DeleteAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var project =
                await _db.Projects
                    .FirstOrDefaultAsync(
                        x => x.Id == projectId,
                        cancellationToken);

            if (project == null)
            {
                return false;
            }

            _db.Projects.Remove(
                project);

            await _db.SaveChangesAsync(
                cancellationToken);

            return true;
        }
    }
}