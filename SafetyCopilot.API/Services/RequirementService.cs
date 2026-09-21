using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Models;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Services;

public class RequirementService
    : IRequirementService
{
    private readonly SafetyDbContext _db;

    public RequirementService(
        SafetyDbContext db)
    {
        _db = db;
    }

    public async Task<
        IReadOnlyList<RequirementResponse>>
        GetProjectRequirementsAsync(
            Guid projectId,
            CancellationToken
                cancellationToken = default)
    {
        return await _db.Requirements
            .AsNoTracking()
            .Where(x =>
                x.Document != null &&
                x.Document.ProjectId ==
                    projectId)
            .OrderBy(x =>
                x.Document!.UploadedAtUtc)
            .ThenBy(x =>
                x.SequenceNumber)
            .Select(x =>
                MapRequirement(x))
            .ToListAsync(
                cancellationToken);
    }

    public async Task<
        IReadOnlyList<RequirementResponse>>
        GetDocumentRequirementsAsync(
            Guid documentId,
            CancellationToken
                cancellationToken = default)
    {
        return await _db.Requirements
            .AsNoTracking()
            .Where(x =>
                x.DocumentId ==
                    documentId)
            .OrderBy(x =>
                x.SequenceNumber)
            .Select(x =>
                MapRequirement(x))
            .ToListAsync(
                cancellationToken);
    }

    public async Task<
        RequirementResponse?>
        GetRequirementAsync(
            Guid requirementId,
            CancellationToken
                cancellationToken = default)
    {
        return await _db.Requirements
            .AsNoTracking()
            .Where(x =>
                x.Id ==
                    requirementId)
            .Select(x =>
                MapRequirement(x))
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<
        HumanClassificationResponse>
        ClassifyAsync(
            Guid requirementId,
            HumanClassificationRequest request,
            CancellationToken
                cancellationToken = default)
    {
        var requirementExists =
            await _db.Requirements
                .AnyAsync(
                    x =>
                        x.Id ==
                        requirementId,
                    cancellationToken);

        if (!requirementExists)
        {
            throw new InvalidOperationException(
                "Requirement not found.");
        }

        var userExists =
            await _db.Users
                .AnyAsync(
                    x =>
                        x.Id ==
                        request.UserId,
                    cancellationToken);

        if (!userExists)
        {
            throw new InvalidOperationException(
                "User not found.");
        }

        var classification =
            await _db
                .HumanClassifications
                .FirstOrDefaultAsync(
                    x =>
                        x.RequirementId ==
                        requirementId,
                    cancellationToken);

        if (classification == null)
        {
            classification =
                new HumanClassification
                {
                    Id =
                        Guid.NewGuid(),

                    RequirementId =
                        requirementId,

                    UserId =
                        request.UserId,

                    IsHazard =
                        request.IsHazard,

                    ClassifiedAtUtc =
                        DateTime.UtcNow
                };

            _db.HumanClassifications
                .Add(classification);
        }
        else
        {
            classification.UserId =
                request.UserId;

            classification.IsHazard =
                request.IsHazard;

            classification
                .ClassifiedAtUtc =
                DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(
            cancellationToken);

        return new HumanClassificationResponse(
            classification.RequirementId,
            classification.UserId,
            classification.IsHazard,
            classification.ClassifiedAtUtc);
    }

    public async Task<
        ClassificationProgressResponse>
        GetProgressAsync(
            Guid projectId,
            Guid userId,
            CancellationToken
                cancellationToken = default)
    {
        var total =
            await _db.Requirements
                .CountAsync(
                    x =>
                        x.Document != null &&
                        x.Document.ProjectId ==
                            projectId,
                    cancellationToken);

        var classified =
            await _db
                .HumanClassifications
                .CountAsync(
                    x =>
                        x.UserId ==
                            userId &&
                        x.Requirement != null &&
                        x.Requirement.Document !=
                            null &&
                        x.Requirement.Document
                            .ProjectId ==
                            projectId,
                    cancellationToken);

        var remaining =
            Math.Max(
                0,
                total - classified);

        var complete =
            total > 0 &&
            classified >= total;

        var percentage =
            total == 0
                ? 0
                : Math.Round(
                    (double)classified /
                    total * 100.0,
                    1);

        return new ClassificationProgressResponse(
            total,
            classified,
            remaining,
            complete,
            percentage);
    }

    private static RequirementResponse
        MapRequirement(
            Requirement x)
    {
        return new RequirementResponse(
            x.Id,
            x.DocumentId,
            x.RequirementNumber,
            x.RequirementText,
            x.SequenceNumber,

            x.HumanClassification ==
                null
                ? null
                : x.HumanClassification
                    .IsHazard,

            x.HumanClassification ==
                null
                ? null
                : x.HumanClassification
                    .ClassifiedAtUtc,

            x.AiClassification !=
                null);
    }
}