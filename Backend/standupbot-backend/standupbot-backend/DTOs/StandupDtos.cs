using System.ComponentModel.DataAnnotations;
using standupbot_backend.Data.Enums;

namespace standupbot_backend.DTOs;

/// <summary>
/// DTO for returning standup data.
/// </summary>
public record StandupDto(
    int Id,
    int UserId,
    string UserName,
    DateOnly Date,
    string JiraId,
    string TaskDescription,
    int PercentageComplete,
    bool HasBlocker,
    string? BlockerDescription,
    BlockerStatus? BlockerStatus,
    string NextTask,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Request for creating a new standup.
/// </summary>
public record CreateStandupRequest(
    [Required]
    [StringLength(50)]
    string JiraId,
    
    [Required]
    [StringLength(500)]
    string TaskDescription,
    
    [Range(0, 100)]
    int PercentageComplete,
    
    bool HasBlocker,
    
    [StringLength(1000)]
    string? BlockerDescription,
    
    [Required]
    [StringLength(500)]
    string NextTask
);

/// <summary>
/// Request for updating an existing standup.
/// </summary>
public record UpdateStandupRequest(
    [StringLength(50)]
    string? JiraId,
    
    [StringLength(500)]
    string? TaskDescription,
    
    [Range(0, 100)]
    int? PercentageComplete,
    
    bool? HasBlocker,
    
    [StringLength(1000)]
    string? BlockerDescription,
    
    [StringLength(500)]
    string? NextTask
);

/// <summary>
/// Summary DTO for list views.
/// </summary>
public record StandupSummaryDto(
    int Id,
    int UserId,
    string UserName,
    DateOnly Date,
    string JiraId,
    int PercentageComplete,
    bool HasBlocker,
    BlockerStatus? BlockerStatus,
    DateTime CreatedAt
);

/// <summary>
/// DTO for updating blocker status (team lead only).
/// </summary>
public record UpdateBlockerStatusRequest(
    [Required]
    BlockerStatus Status
);
