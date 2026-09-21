using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Services
{

    public class AnalysisService
        : IAnalysisService
    {
        private readonly SafetyDbContext _db;

        public AnalysisService(
            SafetyDbContext db)
        {
            _db = db;
        }

        public async Task<
            AnalysisSummaryResponse>
            GetProjectAnalysisAsync(
                Guid projectId,
                CancellationToken cancellationToken = default)
        {
            var rows =
                await _db.Requirements
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.Document != null &&
                            x.Document.ProjectId ==
                            projectId &&
                            x.HumanClassification !=
                                null &&
                            x.AiClassification !=
                                null)
                    .OrderBy(
                        x =>
                            x.Document!
                                .UploadedAtUtc)
                    .ThenBy(
                        x => x.SequenceNumber)
                    .Select(
                        x =>
                            new
                            {
                                x.Id,
                                x.DocumentId,
                                x.RequirementNumber,
                                x.RequirementText,

                                HumanIsHazard =
                                    x.HumanClassification!
                                        .IsHazard,

                                AiIsHazard =
                                    x.AiClassification!
                                        .IsHazard,

                                AiConfidence =
                                    x.AiClassification!
                                        .Confidence,

                                AiExplanation =
                                    x.AiClassification!
                                        .Explanation,

                                AiModelName =
                                    x.AiClassification!
                                        .ModelName
                            })
                    .ToListAsync(
                        cancellationToken);

            var comparisons =
                rows.Select(
                    row =>
                    {
                        var isAgreement =
                            row.HumanIsHazard ==
                            row.AiIsHazard;

                        string category;

                        if (isAgreement)
                        {
                            category =
                                row.HumanIsHazard
                                    ? "AgreementHazard"
                                    : "AgreementNotHazard";
                        }
                        else if (
                            row.HumanIsHazard)
                        {
                            category =
                                "HumanOnlyHazard";
                        }
                        else
                        {
                            category =
                                "AiOnlyHazard";
                        }

                        return new
                            RequirementComparisonResponse(
                                row.Id,
                                row.DocumentId,
                                row.RequirementNumber,
                                row.RequirementText,

                                row.HumanIsHazard,
                                row.AiIsHazard,

                                isAgreement,
                                category,

                                row.AiConfidence,
                                row.AiExplanation,
                                row.AiModelName);
                    })
                .ToList();

            var agreements =
                comparisons
                    .Where(
                        x => x.IsAgreement)
                    .ToList();

            var hazardAgreements =
                comparisons.Count(
                    x =>
                        x.ComparisonCategory ==
                        "AgreementHazard");

            var nonHazardAgreements =
                comparisons.Count(
                    x =>
                        x.ComparisonCategory ==
                        "AgreementNotHazard");

            var humanOnly =
                comparisons
                    .Where(
                        x =>
                            x.ComparisonCategory ==
                            "HumanOnlyHazard")
                    .ToList();

            var aiOnly =
                comparisons
                    .Where(
                        x =>
                            x.ComparisonCategory ==
                            "AiOnlyHazard")
                    .ToList();

            var agreementPercentage =
                comparisons.Count == 0
                    ? 0
                    : Math.Round(
                        agreements.Count *
                        100.0 /
                        comparisons.Count,
                        2);

            return new AnalysisSummaryResponse(
                projectId,

                comparisons.Count,

                agreements.Count,

                hazardAgreements,

                nonHazardAgreements,

                humanOnly.Count,

                aiOnly.Count,

                agreementPercentage,

                agreements,

                humanOnly,

                aiOnly);
        }
    }
}