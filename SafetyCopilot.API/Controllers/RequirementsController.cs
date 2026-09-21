using Microsoft.AspNetCore.Mvc;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Controllers;

[ApiController]
[Route("api")]
public class RequirementsController
    : ControllerBase
{
    private readonly
        IRequirementService
        _requirementService;

    public RequirementsController(
        IRequirementService
            requirementService)
    {
        _requirementService =
            requirementService;
    }

    [HttpGet(
        "projects/{projectId:guid}/requirements")]
    public async Task<
        ActionResult<
            IReadOnlyList<
                RequirementResponse>>>
        GetProjectRequirements(
            Guid projectId,
            CancellationToken
                cancellationToken)
    {
        var requirements =
            await _requirementService
                .GetProjectRequirementsAsync(
                    projectId,
                    cancellationToken);

        return Ok(requirements);
    }

    [HttpGet(
        "documents/{documentId:guid}/requirements")]
    public async Task<
        ActionResult<
            IReadOnlyList<
                RequirementResponse>>>
        GetDocumentRequirements(
            Guid documentId,
            CancellationToken
                cancellationToken)
    {
        var requirements =
            await _requirementService
                .GetDocumentRequirementsAsync(
                    documentId,
                    cancellationToken);

        return Ok(requirements);
    }

    [HttpGet(
        "requirements/{requirementId:guid}")]
    public async Task<
        ActionResult<
            RequirementResponse>>
        GetRequirement(
            Guid requirementId,
            CancellationToken
                cancellationToken)
    {
        var requirement =
            await _requirementService
                .GetRequirementAsync(
                    requirementId,
                    cancellationToken);

        if (requirement == null)
        {
            return NotFound();
        }

        return Ok(requirement);
    }

    [HttpPut(
        "requirements/{requirementId:guid}/human-classification")]
    public async Task<
        ActionResult<
            HumanClassificationResponse>>
        Classify(
            Guid requirementId,
            [FromBody]
            HumanClassificationRequest
                request,
            CancellationToken
                cancellationToken)
    {
        try
        {
            var result =
                await _requirementService
                    .ClassifyAsync(
                        requirementId,
                        request,
                        cancellationToken);

            return Ok(result);
        }
        catch (
            InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    [HttpGet(
        "projects/{projectId:guid}/classification-progress")]
    public async Task<
        ActionResult<
            ClassificationProgressResponse>>
        Progress(
            Guid projectId,
            [FromQuery]
            Guid userId,
            CancellationToken
                cancellationToken)
    {
        var result =
            await _requirementService
                .GetProgressAsync(
                    projectId,
                    userId,
                    cancellationToken);

        return Ok(result);
    }
}