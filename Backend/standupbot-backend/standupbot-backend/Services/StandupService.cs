using Microsoft.EntityFrameworkCore;
using standupbot_backend.Data;
using standupbot_backend.Data.Entities;
using standupbot_backend.Data.Enums;
using standupbot_backend.DTOs;

namespace standupbot_backend.Services;

/// <summary>
/// Service implementation for standup operations.
/// </summary>
public class StandupService(
    StandupBotContext context,
    ILogger<StandupService> logger) : IStandupService
{
    public async Task<StandupDto?> GetTodayStandupAsync(int userId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var standup = await context.Standups
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Date == today, ct);
            
        return standup is null ? null : MapToDto(standup);
    }

    public async Task<IEnumerable<StandupSummaryDto>> GetUserHistoryAsync(
        int userId, 
        int count = 10, 
        CancellationToken ct = default)
    {
        var standups = await context.Standups
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Date)
            .Take(count)
            .ToListAsync(ct);
            
        return standups.Select(MapToSummaryDto);
    }

    public async Task<IEnumerable<StandupDto>> GetTeamStandupsAsync(
        int teamId, 
        DateOnly date, 
        CancellationToken ct = default)
    {
        var standups = await context.Standups
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.User!.TeamId == teamId && s.Date == date)
            .OrderBy(s => s.User!.Name)
            .ToListAsync(ct);
            
        return standups.Select(MapToDto);
    }

    public async Task<StandupDto> CreateAsync(
        int userId, 
        CreateStandupRequest request, 
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        // Check if already submitted today
        var existing = await context.Standups
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Date == today, ct);
            
        if (existing is not null)
        {
            throw new InvalidOperationException("Standup already submitted for today. Use update instead.");
        }
        
        // Validate blocker description if has blocker
        if (request.HasBlocker && string.IsNullOrWhiteSpace(request.BlockerDescription))
        {
            throw new ArgumentException("Blocker description is required when Has Blocker is checked.");
        }
        
        var standup = new Standup
        {
            UserId = userId,
            Date = today,
            JiraId = request.JiraId,
            TaskDescription = request.TaskDescription,
            PercentageComplete = request.PercentageComplete,
            HasBlocker = request.HasBlocker,
            BlockerDescription = request.HasBlocker ? request.BlockerDescription : null,
            BlockerStatus = request.HasBlocker ? BlockerStatus.New : null,
            NextTask = request.NextTask,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Standups.Add(standup);
        await context.SaveChangesAsync(ct);
        
        // Load user for DTO
        await context.Entry(standup).Reference(s => s.User).LoadAsync(ct);
        
        logger.LogInformation("Standup created for user {UserId} on {Date}", userId, today);
        
        return MapToDto(standup);
    }

    public async Task<StandupDto?> UpdateAsync(
        int userId, 
        UpdateStandupRequest request, 
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var standup = await context.Standups
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Date == today, ct);
            
        if (standup is null)
        {
            return null;
        }
        
        // Apply updates (only non-null values)
        if (request.JiraId is not null)
            standup.JiraId = request.JiraId;
        if (request.TaskDescription is not null)
            standup.TaskDescription = request.TaskDescription;
        if (request.PercentageComplete.HasValue)
            standup.PercentageComplete = request.PercentageComplete.Value;
        if (request.HasBlocker.HasValue)
        {
            standup.HasBlocker = request.HasBlocker.Value;
            if (!request.HasBlocker.Value)
            {
                standup.BlockerDescription = null;
                standup.BlockerStatus = null;
            }
        }
        if (request.BlockerDescription is not null && standup.HasBlocker)
            standup.BlockerDescription = request.BlockerDescription;
        if (request.NextTask is not null)
            standup.NextTask = request.NextTask;
            
        // Validate blocker description if has blocker
        if (standup.HasBlocker && string.IsNullOrWhiteSpace(standup.BlockerDescription))
        {
            throw new ArgumentException("Blocker description is required when Has Blocker is checked.");
        }
        
        standup.UpdatedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync(ct);
        
        logger.LogInformation("Standup updated for user {UserId} on {Date}", userId, today);
        
        return MapToDto(standup);
    }

    public async Task<StandupDto?> UpdateBlockerStatusAsync(
        int standupId, 
        BlockerStatus status, 
        CancellationToken ct = default)
    {
        var standup = await context.Standups
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == standupId, ct);
            
        if (standup is null || !standup.HasBlocker)
        {
            return null;
        }
        
        standup.BlockerStatus = status;
        standup.UpdatedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync(ct);
        
        logger.LogInformation("Blocker status updated to {Status} for standup {StandupId}", status, standupId);
        
        return MapToDto(standup);
    }

    public async Task<bool> HasSubmittedTodayAsync(int userId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Standups.AnyAsync(s => s.UserId == userId && s.Date == today, ct);
    }

    public async Task<TeamSubmissionStatusDto> GetTeamSubmissionStatusAsync(
        int teamId, 
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var teamMembers = await context.Users
            .AsNoTracking()
            .Where(u => u.TeamId == teamId)
            .ToListAsync(ct);
            
        var submittedUserIds = await context.Standups
            .AsNoTracking()
            .Where(s => s.User!.TeamId == teamId && s.Date == today)
            .Select(s => s.UserId)
            .ToListAsync(ct);
            
        var submitted = teamMembers
            .Where(u => submittedUserIds.Contains(u.Id))
            .Select(u => new UserSummaryDto(u.Id, u.Name, u.Role));
            
        var pending = teamMembers
            .Where(u => !submittedUserIds.Contains(u.Id))
            .Select(u => new UserSummaryDto(u.Id, u.Name, u.Role));
            
        return new TeamSubmissionStatusDto(
            teamMembers.Count,
            submittedUserIds.Count,
            submitted,
            pending
        );
    }

    private static StandupDto MapToDto(Standup standup) => new(
        standup.Id,
        standup.UserId,
        standup.User?.Name ?? "Unknown",
        standup.Date,
        standup.JiraId,
        standup.TaskDescription,
        standup.PercentageComplete,
        standup.HasBlocker,
        standup.BlockerDescription,
        standup.BlockerStatus,
        standup.NextTask,
        standup.CreatedAt,
        standup.UpdatedAt
    );
    
    private static StandupSummaryDto MapToSummaryDto(Standup standup) => new(
        standup.Id,
        standup.UserId,
        standup.User?.Name ?? "Unknown",
        standup.Date,
        standup.JiraId,
        standup.PercentageComplete,
        standup.HasBlocker,
        standup.BlockerStatus,
        standup.CreatedAt
    );
}
