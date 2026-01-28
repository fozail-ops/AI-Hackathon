using standupbot_backend.Data.Enums;
using standupbot_backend.DTOs;

namespace standupbot_backend.Services;

/// <summary>
/// Service interface for standup operations.
/// </summary>
public interface IStandupService
{
    /// <summary>
    /// Gets today's standup for a user.
    /// </summary>
    Task<StandupDto?> GetTodayStandupAsync(int userId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets standup history for a user (last 10).
    /// </summary>
    Task<IEnumerable<StandupSummaryDto>> GetUserHistoryAsync(int userId, int count = 10, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all standups for a team on a specific date.
    /// </summary>
    Task<IEnumerable<StandupDto>> GetTeamStandupsAsync(int teamId, DateOnly date, CancellationToken ct = default);
    
    /// <summary>
    /// Creates a new standup for today.
    /// </summary>
    Task<StandupDto> CreateAsync(int userId, CreateStandupRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Updates today's standup.
    /// </summary>
    Task<StandupDto?> UpdateAsync(int userId, UpdateStandupRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Updates blocker status (team lead only).
    /// </summary>
    Task<StandupDto?> UpdateBlockerStatusAsync(int standupId, BlockerStatus status, CancellationToken ct = default);
    
    /// <summary>
    /// Checks if user has submitted standup for today.
    /// </summary>
    Task<bool> HasSubmittedTodayAsync(int userId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets team submission status for today.
    /// </summary>
    Task<TeamSubmissionStatusDto> GetTeamSubmissionStatusAsync(int teamId, CancellationToken ct = default);
}

/// <summary>
/// DTO for team submission status.
/// </summary>
public record TeamSubmissionStatusDto(
    int TotalMembers,
    int SubmittedCount,
    IEnumerable<UserSummaryDto> Submitted,
    IEnumerable<UserSummaryDto> Pending
);
