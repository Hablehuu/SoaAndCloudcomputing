using Microsoft.EntityFrameworkCore;

namespace MyRestAPI.Models
{
    public class developerContext : DbContext
    {
        public developerContext(DbContextOptions<developerContext> options)
        : base(options)
        {
        }

        public DbSet<Developer> Developers { get; set; } = null!;

    }
}
