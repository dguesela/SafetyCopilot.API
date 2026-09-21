using Microsoft.AspNetCore.Mvc;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController
    : ControllerBase
{
    private readonly IProjectService
        _projectService;

    public ProjectsController(
        IProjectService projectService)
    {
        _projectService =
            projectService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<ProjectResponse>>>
        GetProjects(
            [FromQuery] Guid userId,
            CancellationToken cancellationToken)
    {
        var projects =
            await _projectService
                .GetByUserAsync(
                    userId,
                    cancellationToken);

        return Ok(projects);
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ProjectResponse>>
        GetProject(
            Guid projectId,
            CancellationToken cancellationToken)
    {
        var project =
            await _projectService
                .GetByIdAsync(
                    projectId,
                    cancellationToken);

        if (project == null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>>
        CreateProject(
            CreateProjectRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var project =
                await _projectService
                    .CreateAsync(
                        request,
                        cancellationToken);

            return CreatedAtAction(
                nameof(GetProject),
                new
                {
                    projectId =
                        project.Id
                },
                project);
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

    [HttpPut("{projectId:guid}")]
    public async Task<ActionResult<ProjectResponse>>
        UpdateProject(
            Guid projectId,
            UpdateProjectRequest request,
            CancellationToken cancellationToken)
    {
        var project =
            await _projectService
                .UpdateAsync(
                    projectId,
                    request,
                    cancellationToken);

        if (project == null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<ActionResult>
        DeleteProject(
            Guid projectId,
            CancellationToken cancellationToken)
    {
        var deleted =
            await _projectService
                .DeleteAsync(
                    projectId,
                    cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}