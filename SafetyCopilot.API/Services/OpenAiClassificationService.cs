using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Models;
using SafetyCopilot.API.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SafetyCopilot.API.Services;

public class OpenAiClassificationService
    : IOpenAiClassificationService
{
    private readonly SafetyDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly ISecretEncryptionService _secretEncryptionService;
    private readonly IConfiguration _configuration;

    public OpenAiClassificationService(
        SafetyDbContext db,
        HttpClient httpClient,
        ISecretEncryptionService secretEncryptionService,
        IConfiguration configuration)
    {
        _db = db;
        _httpClient = httpClient;
        _secretEncryptionService = secretEncryptionService;
        _configuration = configuration;
    }

    public async Task<AiClassificationRunResponse>
        ClassifyProjectAsync(
            Guid projectId,
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        /*
         * ========================================================
         * LOAD USER
         * ========================================================
         */
        var user =
            await _db.Users
                .FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException(
                "The user was not found.");
        }

        if (string.IsNullOrWhiteSpace(
                user.OpenAiApiKeyEncrypted))
        {
            throw new InvalidOperationException(
                "An OpenAI API key has not been configured for this user.");
        }

        /*
         * ========================================================
         * LOAD PROJECT
         * ========================================================
         */
        var projectExists =
            await _db.Projects
                .AnyAsync(
                    x =>
                        x.Id == projectId &&
                        x.UserId == userId,
                    cancellationToken);

        if (!projectExists)
        {
            throw new InvalidOperationException(
                "The project was not found.");
        }

        /*
         * ========================================================
         * LOAD REQUIREMENTS
         * ========================================================
         */
        var requirements =
            await _db.Requirements
                .Include(x =>
                    x.Document)
                .Include(x =>
                    x.HumanClassification)
                .Include(x =>
                    x.AiClassification)
                .Where(x =>
                    x.Document != null &&
                    x.Document.ProjectId == projectId)
                .OrderBy(x =>
                    x.Document!.UploadedAtUtc)
                .ThenBy(x =>
                    x.SequenceNumber)
                .ToListAsync(
                    cancellationToken);

        if (requirements.Count == 0)
        {
            throw new InvalidOperationException(
                "No requirements were found for this project.");
        }

        /*
         * ========================================================
         * ENSURE HUMAN CLASSIFICATION IS COMPLETE
         * ========================================================
         *
         * This is critical to the experimental design.
         *
         * AI classification must remain locked until
         * every requirement has a human classification.
         */
        var unclassifiedCount =
            requirements.Count(
                x =>
                    x.HumanClassification == null);

        if (unclassifiedCount > 0)
        {
            throw new InvalidOperationException(
                $"Human classification is incomplete. " +
                $"{unclassifiedCount} requirement(s) still need classification.");
        }

        /*
         * ========================================================
         * GET API KEY
         * ========================================================
         */
        var apiKey =
            _secretEncryptionService
                .Decrypt(
                    user.OpenAiApiKeyEncrypted);

        if (string.IsNullOrWhiteSpace(
                apiKey))
        {
            throw new InvalidOperationException(
                "The OpenAI API key could not be read.");
        }

        /*
         * ========================================================
         * MODEL
         * ========================================================
         *
         * Keep this configurable so you can record and
         * reproduce experiments with a specific model.
         */
        var model =
            _configuration[
                "OpenAI:Model"];

        if (string.IsNullOrWhiteSpace(
                model))
        {
            model = "gpt-5.6-terra";
        }

        var classifiedCount =
            0;

        /*
         * ========================================================
         * CLASSIFY EACH REQUIREMENT
         * ========================================================
         *
         * We intentionally classify requirements independently.
         *
         * Human classification is NEVER sent to OpenAI.
         */
        foreach (var requirement
                 in requirements)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            var result =
                await ClassifyRequirementAsync(
                    requirement,
                    apiKey,
                    model,
                    cancellationToken);

            /*
             * Upsert AI classification.
             *
             * This allows re-running an experiment before
             * final analysis without duplicate database rows.
             */
            var classification =
                requirement.AiClassification;

            if (classification == null)
            {
                classification =
                    new AiClassification
                    {
                        Id =
                            Guid.NewGuid(),

                        RequirementId =
                            requirement.Id
                    };

                _db.AiClassifications
                    .Add(classification);
            }

            classification.IsHazard =
                result.IsHazard;

            classification.Confidence =
                result.Confidence;

            classification.Explanation =
                result.Explanation;

            classification.ModelName =
                model;

            classification.ClassifiedAtUtc =
                DateTime.UtcNow;

            /*
             * Save each result independently.
             *
             * This means progress is not lost if a later
             * OpenAI request fails.
             */
            await _db.SaveChangesAsync(
                cancellationToken);

            classifiedCount++;
        }

        return new AiClassificationRunResponse(
            projectId,
            requirements.Count,
            classifiedCount,
            model);
    }

    public async Task<AiClassificationResultsResponse?>
    GetProjectResultsAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var projectExists =
            await _db
                .Projects
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == projectId &&
                        x.UserId == userId,
                    cancellationToken);

        if (!projectExists)
        {
            return null;
        }

        var results =
            await _db
                .Requirements
                .AsNoTracking()
                .Where(x =>
                    x.Document != null &&
                    x.Document.ProjectId == projectId &&
                    x.AiClassification != null)
                .OrderBy(x =>
                    x.Document!.UploadedAtUtc)
                .ThenBy(x =>
                    x.SequenceNumber)
                .Select(x =>
                    new AiClassificationDetailsResponse(
                        x.Id,
                        x.RequirementNumber,
                        x.RequirementText,
                        x.AiClassification!.IsHazard,
                        x.AiClassification.Confidence,
                        x.AiClassification.Explanation,
                        x.AiClassification.ModelName,
                        x.AiClassification.ClassifiedAtUtc))
                .ToListAsync(
                    cancellationToken);

        if (results.Count == 0)
        {
            return new AiClassificationResultsResponse(
                projectId,
                0,
                0,
                0,
                null,
                null,
                results);
        }

        var hazardCount =
            results.Count(
                x => x.IsHazard);

        var notHazardCount =
            results.Count -
            hazardCount;

        var confidenceValues =
            results
                .Where(x =>
                    x.Confidence.HasValue)
                .Select(x =>
                    x.Confidence!.Value)
                .ToList();

        double? averageConfidence =
            confidenceValues.Count > 0
                ? confidenceValues.Average()
                : null;

        var modelName =
            results
                .Select(x =>
                    x.ModelName)
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x));

        return new AiClassificationResultsResponse(
            projectId,
            results.Count,
            hazardCount,
            notHazardCount,
            averageConfidence,
            modelName,
            results);
    }


    /*
     * ============================================================
     * CLASSIFY ONE REQUIREMENT
     * ============================================================
     */
    private async Task<AiClassificationResult>
        ClassifyRequirementAsync(
            Requirement requirement,
            string apiKey,
            string model,
            CancellationToken cancellationToken)
    {
        /*
         * IMPORTANT:
         *
         * Do not include HumanClassification.
         *
         * Do not include source [Hazard]/[Non-Hazard]
         * ground-truth labels.
         *
         * The AI sees only the requirement itself.
         */
        var instructions =
            """
            You are acting as an independent software safety
            requirements analyst.

            Classify the provided software requirement as either
            hazard-related or not hazard-related.

            A hazard-related requirement is one that directly
            contributes to preventing, detecting, controlling,
            mitigating, responding to, or recovering from an
            unsafe system state, hazardous event, safety-critical
            failure, accident condition, or loss of a safety
            function.

            A requirement should not be classified as hazard-related
            merely because it occurs in a safety-critical system.
            General business, usability, reporting, administrative,
            or ordinary functional behavior should be classified as
            not hazard-related unless it has a direct safety role.

            Make the classification independently.

            Return:
            - isHazard: true or false
            - confidence: number between 0 and 1
            - explanation: concise justification for the decision

            Do not infer or mention any human classification.
            """;

        var requirementInput =
            $"""
            Requirement identifier:
            {requirement.RequirementNumber ?? "Unknown"}

            Requirement:
            {requirement.RequirementText}
            """;

        /*
         * Responses API structured output.
         *
         * JSON schema keeps the response predictable
         * and easier to parse than free-form text.
         */
        var requestBody =
            new
            {
                model,

                store = false,

                instructions,

                input = requirementInput,

                text = new
                {
                    format = new
                    {
                        type =
                            "json_schema",

                        name =
                            "hazard_classification",

                        strict =
                            true,

                        schema = new
                        {
                            type =
                                "object",

                            properties =
                                new
                                {
                                    isHazard =
                                        new
                                        {
                                            type =
                                                "boolean"
                                        },

                                    confidence =
                                        new
                                        {
                                            type =
                                                "number",

                                            minimum =
                                                0,

                                            maximum =
                                                1
                                        },

                                    explanation =
                                        new
                                        {
                                            type =
                                                "string"
                                        }
                                },

                            required =
                                new[]
                                {
                                    "isHazard",
                                    "confidence",
                                    "explanation"
                                },

                            additionalProperties =
                                false
                        }
                    }
                }
            };

        var json =
            JsonSerializer.Serialize(
                requestBody);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "v1/responses");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        var responseText =
            await response.Content
                .ReadAsStringAsync(
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenAI request failed with status " +
                $"{(int)response.StatusCode}. " +
                $"{ExtractOpenAiError(responseText)}");
        }

        return ParseResponse(
            responseText);
    }

    /*
     * ============================================================
     * PARSE OPENAI RESPONSE
     * ============================================================
     */
    private static AiClassificationResult
        ParseResponse(
            string responseText)
    {
        using var document =
            JsonDocument.Parse(
                responseText);

        var root =
            document.RootElement;

        /*
         * Responses API structure:
         *
         * output[]
         *   -> message
         *      -> content[]
         *         -> output_text
         *            -> text
         *
         * The text itself contains the JSON generated
         * according to our schema.
         */
        if (!root.TryGetProperty(
                "output",
                out var output) ||
            output.ValueKind !=
                JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                "OpenAI returned an unexpected response format.");
        }

        string? structuredJson =
            null;

        foreach (var outputItem
                 in output.EnumerateArray())
        {
            if (!outputItem.TryGetProperty(
                    "content",
                    out var content) ||
                content.ValueKind !=
                    JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem
                     in content.EnumerateArray())
            {
                if (!contentItem.TryGetProperty(
                        "type",
                        out var typeElement))
                {
                    continue;
                }

                var type =
                    typeElement
                        .GetString();

                if (!string.Equals(
                        type,
                        "output_text",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!contentItem.TryGetProperty(
                        "text",
                        out var textElement))
                {
                    continue;
                }

                structuredJson =
                    textElement
                        .GetString();

                if (!string.IsNullOrWhiteSpace(
                        structuredJson))
                {
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(
                    structuredJson))
            {
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(
                structuredJson))
        {
            throw new InvalidOperationException(
                "OpenAI did not return a classification result.");
        }

        using var classificationJson =
            JsonDocument.Parse(
                structuredJson);

        var classificationRoot =
            classificationJson
                .RootElement;

        var isHazard =
            classificationRoot
                .GetProperty(
                    "isHazard")
                .GetBoolean();

        var confidence =
            classificationRoot
                .GetProperty(
                    "confidence")
                .GetDouble();

        var explanation =
            classificationRoot
                .GetProperty(
                    "explanation")
                .GetString();

        confidence =
            Math.Clamp(
                confidence,
                0,
                1);

        return new AiClassificationResult(
            isHazard,
            confidence,
            explanation ?? string.Empty);
    }

    /*
     * ============================================================
     * OPENAI ERROR HANDLING
     * ============================================================
     */
    private static string
        ExtractOpenAiError(
            string responseText)
    {
        if (string.IsNullOrWhiteSpace(
                responseText))
        {
            return "No error details were returned.";
        }

        try
        {
            using var document =
                JsonDocument.Parse(
                    responseText);

            if (document.RootElement
                .TryGetProperty(
                    "error",
                    out var error))
            {
                if (error.TryGetProperty(
                        "message",
                        out var message))
                {
                    return message
                               .GetString()
                           ?? responseText;
                }
            }
        }
        catch
        {
            // Ignore parsing failure.
        }

        return responseText;
    }

    private sealed record
        AiClassificationResult(
            bool IsHazard,
            double Confidence,
            string Explanation);
}