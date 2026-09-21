using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Models;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Services;

public class RequirementDocumentService
    : IRequirementDocumentService
{
    private readonly SafetyDbContext _db;
    private readonly IPdfTextService _pdfTextService;
    private readonly IRequirementParserService _requirementParser;

    public RequirementDocumentService(
        SafetyDbContext db,
        IPdfTextService pdfTextService,
        IRequirementParserService requirementParser)
    {
        _db = db;
        _pdfTextService = pdfTextService;
        _requirementParser = requirementParser;
    }

    /*
     * ============================================================
     * GET PROJECT DOCUMENTS
     * ============================================================
     */
    public async Task<IReadOnlyList<RequirementDocumentResponse>>
        GetProjectDocumentsAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
    {
        return await _db
            .RequirementDocuments
            .AsNoTracking()
            .Where(x =>
                x.ProjectId == projectId)
            .OrderByDescending(x =>
                x.UploadedAtUtc)
            .Select(x =>
                new RequirementDocumentResponse(
                    x.Id,
                    x.ProjectId,
                    x.FileName,
                    x.ContentType,
                    x.FileSizeBytes,
                    x.UploadedAtUtc,
                    x.ProcessedAtUtc,
                    x.Requirements.Count))
            .ToListAsync(
                cancellationToken);
    }

    /*
     * ============================================================
     * GET DOCUMENT
     * ============================================================
     */
    public async Task<RequirementDocumentDetailsResponse?>
        GetDocumentAsync(
            Guid documentId,
            CancellationToken cancellationToken = default)
    {
        return await _db
            .RequirementDocuments
            .AsNoTracking()
            .Where(x =>
                x.Id == documentId)
            .Select(x =>
                new RequirementDocumentDetailsResponse(
                    x.Id,
                    x.ProjectId,
                    x.FileName,
                    x.ContentType,
                    x.FileSizeBytes,
                    x.ExtractedText,
                    x.UploadedAtUtc,
                    x.ProcessedAtUtc,
                    x.Requirements.Count))
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    /*
     * ============================================================
     * UPLOAD DOCUMENT
     * ============================================================
     *
     * Upload only stores the PDF.
     *
     * Requirement extraction is intentionally performed
     * separately when the user clicks:
     *
     *     Extract Requirements
     *
     * on the frontend.
     */
    public async Task<RequirementDocumentResponse>
        UploadAsync(
            Guid projectId,
            IFormFile file,
            CancellationToken cancellationToken = default)
    {
        /*
         * Make sure the project exists.
         */
        var projectExists =
            await _db
                .Projects
                .AnyAsync(
                    x =>
                        x.Id == projectId,
                    cancellationToken);

        if (!projectExists)
        {
            throw new InvalidOperationException(
                "The project does not exist.");
        }

        /*
         * Validate the uploaded file.
         */
        if (file == null ||
            file.Length == 0)
        {
            throw new InvalidOperationException(
                "A PDF file is required.");
        }

        var extension =
            Path.GetExtension(
                file.FileName);

        if (!string.Equals(
                extension,
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only PDF documents are supported.");
        }

        /*
         * Maximum file size:
         *
         * 50 MB
         */
        const long maxFileSize =
            50_000_000;

        if (file.Length > maxFileSize)
        {
            throw new InvalidOperationException(
                "The PDF file exceeds the 50 MB upload limit.");
        }

        /*
         * Read the PDF into memory.
         */
        byte[] pdfContent;

        await using (
            var memoryStream =
                new MemoryStream())
        {
            await file.CopyToAsync(
                memoryStream,
                cancellationToken);

            pdfContent =
                memoryStream.ToArray();
        }

        var now =
            DateTime.UtcNow;

        /*
         * Create the document.
         *
         * ExtractedText remains null because
         * extraction occurs later.
         */
        var document =
            new RequirementDocument
            {
                Id =
                    Guid.NewGuid(),

                ProjectId =
                    projectId,

                FileName =
                    Path.GetFileName(
                        file.FileName),

                ContentType =
                    string.IsNullOrWhiteSpace(
                        file.ContentType)
                        ? "application/pdf"
                        : file.ContentType,

                FileSizeBytes =
                    file.Length,

                PdfContent =
                    pdfContent,

                ExtractedText =
                    null,

                UploadedAtUtc =
                    now,

                ProcessedAtUtc =
                    null
            };

        _db
            .RequirementDocuments
            .Add(document);

        await _db
            .SaveChangesAsync(
                cancellationToken);

        return new RequirementDocumentResponse(
            document.Id,
            document.ProjectId,
            document.FileName,
            document.ContentType,
            document.FileSizeBytes,
            document.UploadedAtUtc,
            document.ProcessedAtUtc,
            0);
    }

    /*
     * ============================================================
     * EXTRACT REQUIREMENTS
     * ============================================================
     *
     * This method:
     *
     * 1. Loads the uploaded PDF.
     * 2. Makes sure classification has not started.
     * 3. Re-extracts text directly from PdfContent.
     * 4. Parses REQ-... requirements.
     * 5. Removes previous unclassified requirements.
     * 6. Inserts the newly extracted requirements.
     * 7. Stores the new ExtractedText.
     *
     * IMPORTANT:
     *
     * We ALWAYS re-extract PdfContent.
     *
     * We do NOT reuse document.ExtractedText because an
     * earlier version of PdfTextService may have produced
     * corrupted text such as:
     *
     *     Thesystemshallprovide...
     *
     * Re-extraction ensures the latest PDF extraction
     * algorithm is used.
     */
    public async Task<RequirementExtractionResponse>
        ExtractRequirementsAsync(
            Guid documentId,
            CancellationToken cancellationToken = default)
    {
        /*
         * Load document and existing requirements.
         */
        var document =
            await _db
                .RequirementDocuments
                .Include(x =>
                    x.Requirements)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == documentId,
                    cancellationToken);

        if (document == null)
        {
            throw new InvalidOperationException(
                "The requirements document was not found.");
        }

        /*
         * Verify that the original PDF is still available.
         */
        if (document.PdfContent == null ||
            document.PdfContent.Length == 0)
        {
            throw new InvalidOperationException(
                "The uploaded PDF document does not contain file data.");
        }

        /*
         * ========================================================
         * PROTECT EXPERIMENTAL DATA
         * ========================================================
         *
         * Re-extraction is allowed only before either the
         * human or AI classification phase has started.
         *
         * Otherwise deleting/recreating requirements would
         * invalidate existing classification records.
         */
        if (document.Requirements.Count > 0)
        {
            var requirementIds =
                document
                    .Requirements
                    .Select(x =>
                        x.Id)
                    .ToList();

            var hasHumanClassifications =
                await _db
                    .HumanClassifications
                    .AnyAsync(
                        x =>
                            requirementIds.Contains(
                                x.RequirementId),
                        cancellationToken);

            var hasAiClassifications =
                await _db
                    .AiClassifications
                    .AnyAsync(
                        x =>
                            requirementIds.Contains(
                                x.RequirementId),
                        cancellationToken);

            if (hasHumanClassifications ||
                hasAiClassifications)
            {
                throw new InvalidOperationException(
                    "Requirements cannot be re-extracted because classification has already started for this document.");
            }
        }

        /*
         * ========================================================
         * ALWAYS RE-EXTRACT THE ORIGINAL PDF
         * ========================================================
         *
         * Do NOT do:
         *
         * var extractedText = document.ExtractedText;
         *
         * because that could reuse text generated by an older
         * PDF extraction algorithm.
         */
        var extractedText =
            await _pdfTextService
                .ExtractTextAsync(
                    document.PdfContent,
                    cancellationToken);

        if (string.IsNullOrWhiteSpace(
                extractedText))
        {
            throw new InvalidOperationException(
                "No readable text could be extracted from the PDF document.");
        }

        /*
         * Store the newest extracted text.
         */
        document.ExtractedText =
            extractedText;

        /*
         * ========================================================
         * PARSE REQUIREMENTS
         * ========================================================
         */
        var parsedRequirements =
            _requirementParser
                .ParseRequirements(
                    extractedText);

        if (parsedRequirements.Count == 0)
        {
            document.ProcessedAtUtc =
                DateTime.UtcNow;

            await _db
                .SaveChangesAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                "Text was extracted from the PDF, but no individual requirements could be identified.");
        }

        /*
         * ========================================================
         * REMOVE EXISTING UNCLASSIFIED REQUIREMENTS
         * ========================================================
         *
         * This allows the user to re-run extraction while
         * developing/testing the parser.
         *
         * We already verified above that no human or AI
         * classifications exist.
         */
        if (document.Requirements.Count > 0)
        {
            _db
                .Requirements
                .RemoveRange(
                    document.Requirements);

            /*
             * Save deletion first.
             *
             * tblRequirement has unique indexes involving:
             *
             * DocumentId + SequenceNumber
             *
             * and potentially:
             *
             * DocumentId + RequirementNumber
             *
             * Saving deletion before insertion prevents
             * unique-key collisions.
             */
            await _db
                .SaveChangesAsync(
                    cancellationToken);

            /*
             * Clear the in-memory navigation collection.
             */
            document
                .Requirements
                .Clear();
        }

        /*
         * ========================================================
         * CREATE NEW REQUIREMENTS
         * ========================================================
         */
        var now =
            DateTime.UtcNow;

        /*
         * Protect against duplicate requirement numbers
         * produced by malformed PDF extraction.
         */
        var seenRequirementNumbers =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        var sequenceNumber =
            1;

        foreach (
            var parsedRequirement
            in parsedRequirements)
        {
            var requirementNumber =
                NormalizeRequirementNumber(
                    parsedRequirement
                        .RequirementNumber);

            var requirementText =
                parsedRequirement
                    .RequirementText
                    .Trim();

            /*
             * Skip empty requirements.
             */
            if (string.IsNullOrWhiteSpace(
                    requirementText))
            {
                continue;
            }

            /*
             * If the requirement has an ID,
             * prevent duplicate IDs.
             */
            if (!string.IsNullOrWhiteSpace(
                    requirementNumber))
            {
                if (!seenRequirementNumbers.Add(
                        requirementNumber))
                {
                    continue;
                }
            }

            var requirement =
                new Requirement
                {
                    Id =
                        Guid.NewGuid(),

                    DocumentId =
                        document.Id,

                    RequirementNumber =
                        requirementNumber,

                    RequirementText =
                        requirementText,

                    /*
                     * Use our own sequential value instead
                     * of trusting the parser after duplicate
                     * filtering.
                     */
                    SequenceNumber =
                        sequenceNumber,

                    CreatedAtUtc =
                        now
                };

            _db
                .Requirements
                .Add(requirement);

            sequenceNumber++;
        }

        /*
         * Mark document as processed.
         */
        document.ProcessedAtUtc =
            now;

        /*
         * Save new requirements and updated
         * extracted text.
         */
        await _db
            .SaveChangesAsync(
                cancellationToken);

        /*
         * ========================================================
         * VERIFY DATABASE RESULT
         * ========================================================
         */
        var requirementCount =
            await _db
                .Requirements
                .CountAsync(
                    x =>
                        x.DocumentId ==
                        document.Id,
                    cancellationToken);

        if (requirementCount == 0)
        {
            throw new InvalidOperationException(
                "Requirement extraction completed, but no valid requirement records were created.");
        }

        return new RequirementExtractionResponse(
            document.Id,
            document.FileName,
            requirementCount);
    }

    /*
     * ============================================================
     * DELETE DOCUMENT
     * ============================================================
     */
    public async Task<bool>
        DeleteAsync(
            Guid documentId,
            CancellationToken cancellationToken = default)
    {
        var document =
            await _db
                .RequirementDocuments
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == documentId,
                    cancellationToken);

        if (document == null)
        {
            return false;
        }

        _db
            .RequirementDocuments
            .Remove(document);

        await _db
            .SaveChangesAsync(
                cancellationToken);

        return true;
    }

    /*
     * ============================================================
     * NORMALIZE REQUIREMENT NUMBER
     * ============================================================
     */
    private static string?
        NormalizeRequirementNumber(
            string? value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return null;
        }

        var normalized =
            value
                .Trim()
                .ToUpperInvariant();

        /*
         * RequirementNumber is NVARCHAR(100).
         */
        if (normalized.Length > 100)
        {
            normalized =
                normalized[..100];
        }

        return normalized;
    }
}