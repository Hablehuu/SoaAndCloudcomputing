using Microsoft.EntityFrameworkCore;

namespace MyRestAPI.Models 
{
    public class GameContext : DbContext
    {
        public GameContext(DbContextOptions<GameContext> options)
        : base(options)
        {
        }

        public DbSet<Game> Games { get; set; } = null!;

    }
}
