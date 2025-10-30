using Empath_AI.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Empath_AI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Devices> Devices { get; set; }
        public DbSet<HeartRateRecord> Hearts { get; set; }

    }
}
