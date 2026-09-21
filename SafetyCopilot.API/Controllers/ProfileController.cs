using Microsoft.AspNetCore.Mvc;
using SafetyCopilot.API.DTOs;
using SafetyCopilot.API.Services.Interfaces;

namespace SafetyCopilot.API.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController
    : ControllerBase
{
    private readonly IProfileService
        _profileService;

    public ProfileController(
        IProfileService profileService)
    {
        _profileService =
            profileService;
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ProfileResponse>>
        Get(
            Guid userId,
            CancellationToken cancellationToken)
    {
        var profile =
            await _profileService.GetAsync(
                userId,
                cancellationToken);

        if (profile == null)
        {
            return NotFound(
                new
                {
                    message =
                        "User not found."
                });
        }

        return Ok(profile);
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<ProfileResponse>>
        Update(
            Guid userId,
            UpdateProfileRequest request,
            CancellationToken cancellationToken)
    {
        var profile =
            await _profileService.UpdateAsync(
                userId,
                request,
                cancellationToken);

        if (profile == null)
        {
            return NotFound(
                new
                {
                    message =
                        "User not found."
                });
        }

        return Ok(profile);
    }

    [HttpPut("{userId:guid}/api-key")]
    public async Task<ActionResult>
        SaveApiKey(
            Guid userId,
            SaveOpenAiApiKeyRequest request,
            CancellationToken cancellationToken)
    {
        var saved =
            await _profileService
                .SaveApiKeyAsync(
                    userId,
                    request.ApiKey,
                    cancellationToken);

        if (!saved)
        {
            return NotFound(
                new
                {
                    message =
                        "User not found."
                });
        }

        return Ok(
            new
            {
                message =
                    "OpenAI API key saved.",
                isConfigured =
                    true
            });
    }

    [HttpDelete("{userId:guid}/api-key")]
    public async Task<ActionResult>
        RemoveApiKey(
            Guid userId,
            CancellationToken cancellationToken)
    {
        var removed =
            await _profileService
                .RemoveApiKeyAsync(
                    userId,
                    cancellationToken);

        if (!removed)
        {
            return NotFound(
                new
                {
                    message =
                        "User not found."
                });
        }

        return NoContent();
    }
}