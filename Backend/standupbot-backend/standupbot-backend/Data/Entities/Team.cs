using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace standupbot_backend.Data.Entities;

/// <summary>
/// Represents a team in the organization.
/// </summary>
[Table("teams")]
public class Team
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("name")]
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<User> Members { get; set; } = [];
}
