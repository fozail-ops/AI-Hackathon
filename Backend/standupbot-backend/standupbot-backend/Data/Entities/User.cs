using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using standupbot_backend.Data.Enums;

namespace standupbot_backend.Data.Entities;

/// <summary>
/// Represents a user in the StandupBot system.
/// </summary>
[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("name")]
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }
    
    [Column("email")]
    [Required]
    [StringLength(100)]
    public required string Email { get; set; }
    
    [Column("role")]
    public UserRole Role { get; set; } = UserRole.Member;
    
    [Column("team_id")]
    public int TeamId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Team? Team { get; set; }
    public virtual ICollection<Standup> Standups { get; set; } = [];
}
