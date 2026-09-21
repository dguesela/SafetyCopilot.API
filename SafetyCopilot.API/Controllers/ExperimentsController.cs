using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services.Interfaces;
using System.Security.Claims;

namespace SafetyCopilot.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/experiments")]
    public class ExperimentsController : ControllerBase
    {
        private readonly IExperimentService _service;

        public ExperimentsController(
            IExperimentService service)
        {
            _service = service;
        }

        private Guid UserId =>
            Guid.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

        [HttpPost("sessions")]
        public async Task<ActionResult>
            CreateSession(
                CreateExperimentSessionRequest request)
        {
            try
            {
                return Ok(
                    await _service.CreateAsync(
                        UserId,
                        request));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPost(
            "sessions/{sessionId:guid}/complete")]
        public async Task<ActionResult>
            CompleteSession(Guid sessionId)
        {
            var result =
                await _service.CompleteAsync(
                    UserId,
                    sessionId);

            return result == null
                ? NotFound()
                : Ok(result);
        }

        [HttpPost(
            "sessions/{sessionId:guid}/measurements")]
        public async Task<IActionResult>
            RecordMeasurement(
                Guid sessionId,
                RecordMeasurementRequest request)
        {
            try
            {
                await _service.RecordMeasurementAsync(
                    UserId,
                    sessionId,
                    request);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    new { message = ex.Message });
            }
        }
    }
}