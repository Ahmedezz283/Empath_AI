using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Empath_AI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

       
    }
}
