using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Controllers;

[ApiController]
[Route("api")]
public class RequirementDocumentsController : ControllerBase
{
    private readonly IRequirementDocumentService _documentService;
    private readonly SafetyDbContext _db;

    public RequirementDocumentsController(
        IRequirementDocumentService documentService,
        SafetyDbContext db)
    {
        _documentService = documentService;
        _db = db;
    }

    // =========================================================
    // GET: /api/projects/{projectId}/documents
    // =========================================================

    [HttpGet("projects/{projectId:guid}/documents")]
    public async Task<ActionResult<
        IReadOnlyList<RequirementDocumentResponse>>>
        GetProjectDocuments(
            Guid projectId,
            CancellationToken cancellationToken)
    {
        var documents =
            await _documentService
                .GetProjectDocumentsAsync(
                    projectId,
                    cancellationToken);

        return Ok(documents);
    }

    // =========================================================
    // POST: /api/projects/{projectId}/documents
    // =========================================================

    [HttpPost("projects/{projectId:guid}/documents")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<RequirementDocumentResponse>>
        Upload(
            Guid projectId,
            [FromForm] UploadRequirementDocumentRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            if (request.File == null)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "A PDF requirements document is required."
                    });
            }

            var result =
                await _documentService
                    .UploadAsync(
                        projectId,
                        request.File,
                        cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Unable to upload the requirements document.",
                    detail = ex.Message
                });
        }
    }

    // =========================================================
    // POST:
    // /api/documents/{documentId}/extract-requirements
    // =========================================================

    [HttpPost(
        "documents/{documentId:guid}/extract-requirements")]
    public async Task<ActionResult<RequirementExtractionResponse>>
        ExtractRequirements(
            Guid documentId,
            CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _documentService
                    .ExtractRequirementsAsync(
                        documentId,
                        cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Unable to extract requirements from the document.",
                    detail = ex.Message
                });
        }
    }

    // =========================================================
    // GET: /api/documents/{documentId}
    // =========================================================

    [HttpGet("documents/{documentId:guid}")]
    public async Task<ActionResult<
        RequirementDocumentDetailsResponse>>
        GetDocument(
            Guid documentId,
            CancellationToken cancellationToken)
    {
        var document =
            await _documentService
                .GetDocumentAsync(
                    documentId,
                    cancellationToken);

        if (document == null)
        {
            return NotFound(
                new
                {
                    message =
                        "Requirements document was not found."
                });
        }

        return Ok(document);
    }

    // =========================================================
    // GET: /api/documents/{documentId}/pdf
    // =========================================================

    [HttpGet("documents/{documentId:guid}/pdf")]
    public async Task<IActionResult>
        GetPdf(
            Guid documentId,
            CancellationToken cancellationToken)
    {
        var document =
            await _db
                .RequirementDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == documentId,
                    cancellationToken);

        if (document == null)
        {
            return NotFound(
                new
                {
                    message =
                        "Requirements document was not found."
                });
        }

        if (document.PdfContent == null ||
            document.PdfContent.Length == 0)
        {
            return NotFound(
                new
                {
                    message =
                        "The PDF file content is unavailable."
                });
        }

        return File(
            document.PdfContent,
            document.ContentType,
            document.FileName);
    }

    // =========================================================
    // DELETE: /api/documents/{documentId}
    // =========================================================

    [HttpDelete("documents/{documentId:guid}")]
    public async Task<IActionResult>
        Delete(
            Guid documentId,
            CancellationToken cancellationToken)
    {
        try
        {
            var deleted =
                await _documentService
                    .DeleteAsync(
                        documentId,
                        cancellationToken);

            if (!deleted)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Requirements document was not found."
                    });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Unable to delete the requirements document.",
                    detail = ex.Message
                });
        }
    }
}