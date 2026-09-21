using Microsoft.AspNetCore.Mvc;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/analysis")]
public class AnalysisController
    : ControllerBase
{
    private readonly IAnalysisService
        _analysisService;

    public AnalysisController(
        IAnalysisService analysisService)
    {
        _analysisService =
            analysisService;
    }

    [HttpGet]
    public async Task<ActionResult<
        AnalysisSummaryResponse>>
        GetAnalysis(
            Guid projectId,
            CancellationToken cancellationToken)
    {
        var analysis =
            await _analysisService
                .GetProjectAnalysisAsync(
                    projectId,
                    cancellationToken);

        return Ok(analysis);
    }
}