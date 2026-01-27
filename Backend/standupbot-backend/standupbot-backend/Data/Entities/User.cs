using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace standupbot_backend.Data.Entities;

[Table("user")]
public class User
{
    [Column("id")] public Guid Id { get; set; }
    [Column("name"), StringLength(100)] public string Name { get; set; } = string.Empty;
    [Column("email"), StringLength(75)] public string Email { get; set; } = string.Empty;

}
