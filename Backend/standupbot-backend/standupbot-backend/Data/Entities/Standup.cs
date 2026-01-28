using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using standupbot_backend.Data.Enums;

namespace standupbot_backend.Data.Entities;

/// <summary>
/// Represents a daily standup entry from a team member.
/// </summary>
[Table("standups")]
public class Standup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("date")]
    public DateOnly Date { get; set; }
    
    [Column("jira_id")]
    [Required]
    [StringLength(50)]
    public required string JiraId { get; set; }
    
    [Column("task_description")]
    [Required]
    [StringLength(500)]
    public required string TaskDescription { get; set; }
    
    [Column("percentage_complete")]
    [Range(0, 100)]
    public int PercentageComplete { get; set; }
    
    [Column("has_blocker")]
    public bool HasBlocker { get; set; }
    
    [Column("blocker_description")]
    [StringLength(1000)]
    public string? BlockerDescription { get; set; }
    
    [Column("blocker_status")]
    public BlockerStatus? BlockerStatus { get; set; }
    
    [Column("next_task")]
    [Required]
    [StringLength(500)]
    public required string NextTask { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual User? User { get; set; }
}
