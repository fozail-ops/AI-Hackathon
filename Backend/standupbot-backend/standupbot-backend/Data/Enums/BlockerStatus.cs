using System.ComponentModel.DataAnnotations;

namespace standupbot_backend.Data.Enums;

/// <summary>
/// Status of a blocker in the standup.
/// </summary>
public enum BlockerStatus
{
    [Display(Name = "New", Description = "Newly reported blocker")]
    New = 0,
    
    [Display(Name = "Critical", Description = "Marked as critical by team lead")]
    Critical = 1,
    
    [Display(Name = "Resolved", Description = "Blocker has been resolved")]
    Resolved = 2
}
