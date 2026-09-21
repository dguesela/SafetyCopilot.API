using Microsoft.AspNetCore.Mvc;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services;

namespace SafetyCopilot.API.Controllers;

[ApiController]
[Route("api")]
public class AiClassificationController
    : ControllerBase
{
    private readonly IOpenAiClassificationService
        _classificationService;

    public AiClassificationController(
        IOpenAiClassificationService classificationService)
    {
        _classificationService =
            classificationService;
    }

    /*
     * ============================================================
     * RUN AI CLASSIFICATION
     * ============================================================
     */
    [HttpPost(
        "projects/{projectId:guid}/ai-classification")]
    public async Task<ActionResult<AiClassificationRunResponse>>
        RunClassification(
            Guid projectId,
            [FromBody] RunAiClassificationRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _classificationService
                    .ClassifyProjectAsync(
                        projectId,
                        request.UserId,
                        cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Unable to run AI classification.",

                    detail =
                        ex.Message
                });
        }
    }

    /*
     * ============================================================
     * GET AI CLASSIFICATION RESULTS
     * ============================================================
     */
    [HttpGet(
        "projects/{projectId:guid}/ai-classifications")]
    public async Task<ActionResult<AiClassificationResultsResponse>>
        GetClassificationResults(
            Guid projectId,
            [FromQuery] Guid userId,
            CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _classificationService
                    .GetProjectResultsAsync(
                        projectId,
                        userId,
                        cancellationToken);

            if (result == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "The project was not found."
                    });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Unable to load AI classification results.",

                    detail =
                        ex.Message
                });
        }
    }
}