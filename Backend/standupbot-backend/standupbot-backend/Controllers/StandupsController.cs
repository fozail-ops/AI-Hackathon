using Microsoft.AspNetCore.Mvc;
using standupbot_backend.DTOs;
using standupbot_backend.Services;

namespace standupbot_backend.Controllers;

/// <summary>
/// API controller for standup operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StandupsController(
    IStandupService standupService,
    ILogger<StandupsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets today's standup for a user.
    /// </summary>
    [HttpGet("today/{userId:int}")]
    [ProducesResponseType(typeof(StandupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandupDto>> GetTodayStandup(
        int userId, 
        CancellationToken ct)
    {
        var standup = await standupService.GetTodayStandupAsync(userId, ct);
        return standup is null ? NotFound() : Ok(standup);
    }

    /// <summary>
    /// Gets standup history for a user (last 10 by default).
    /// </summary>
    [HttpGet("history/{userId:int}")]
    [ProducesResponseType(typeof(IEnumerable<StandupSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StandupSummaryDto>>> GetUserHistory(
        int userId,
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var standups = await standupService.GetUserHistoryAsync(userId, count, ct);
        return Ok(standups);
    }

    /// <summary>
    /// Gets all standups for a team on a specific date.
    /// </summary>
    [HttpGet("team/{teamId:int}")]
    [ProducesResponseType(typeof(IEnumerable<StandupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StandupDto>>> GetTeamStandups(
        int teamId,
        [FromQuery] DateOnly? date,
        CancellationToken ct)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var standups = await standupService.GetTeamStandupsAsync(teamId, targetDate, ct);
        return Ok(standups);
    }

    /// <summary>
    /// Creates a new standup for today.
    /// </summary>
    [HttpPost("{userId:int}")]
    [ProducesResponseType(typeof(StandupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StandupDto>> Create(
        int userId,
        [FromBody] CreateStandupRequest request,
        CancellationToken ct)
    {
        try
        {
            var standup = await standupService.CreateAsync(userId, request, ct);
            return CreatedAtAction(
                nameof(GetTodayStandup), 
                new { userId }, 
                standup
            );
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates today's standup for a user.
    /// </summary>
    [HttpPut("{userId:int}")]
    [ProducesResponseType(typeof(StandupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandupDto>> Update(
        int userId,
        [FromBody] UpdateStandupRequest request,
        CancellationToken ct)
    {
        try
        {
            var standup = await standupService.UpdateAsync(userId, request, ct);
            return standup is null ? NotFound() : Ok(standup);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates blocker status (team lead only).
    /// </summary>
    [HttpPatch("{standupId:int}/blocker-status")]
    [ProducesResponseType(typeof(StandupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandupDto>> UpdateBlockerStatus(
        int standupId,
        [FromBody] UpdateBlockerStatusRequest request,
        CancellationToken ct)
    {
        var standup = await standupService.UpdateBlockerStatusAsync(standupId, request.Status, ct);
        return standup is null ? NotFound() : Ok(standup);
    }

    /// <summary>
    /// Checks if user has submitted standup for today.
    /// </summary>
    [HttpGet("status/{userId:int}")]
    [ProducesResponseType(typeof(SubmissionStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubmissionStatusResponse>> GetSubmissionStatus(
        int userId,
        CancellationToken ct)
    {
        var hasSubmitted = await standupService.HasSubmittedTodayAsync(userId, ct);
        return Ok(new SubmissionStatusResponse(hasSubmitted));
    }

    /// <summary>
    /// Gets team submission status for today.
    /// </summary>
    [HttpGet("team/{teamId:int}/status")]
    [ProducesResponseType(typeof(TeamSubmissionStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TeamSubmissionStatusDto>> GetTeamSubmissionStatus(
        int teamId,
        CancellationToken ct)
    {
        var status = await standupService.GetTeamSubmissionStatusAsync(teamId, ct);
        return Ok(status);
    }
}

/// <summary>
/// Response for submission status check.
/// </summary>
public record SubmissionStatusResponse(bool HasSubmittedToday);
