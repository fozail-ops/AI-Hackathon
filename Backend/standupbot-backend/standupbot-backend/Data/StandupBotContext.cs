using Microsoft.EntityFrameworkCore;
using standupbot_backend.Data.Entities;

namespace standupbot_backend.Data;

public class StandupBotContext(DbContextOptions<StandupBotContext> options) : DbContext(options)
{
    public DbSet<User> StandupEntries { get; set; } = null!;
}
