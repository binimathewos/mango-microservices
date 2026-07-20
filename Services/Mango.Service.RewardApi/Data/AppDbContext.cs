using Microsoft.EntityFrameworkCore;
using Mango.Service.RewardApi.Models;

namespace Mango.Service.RewardApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Rewards> Rewards { get; set; }
    }
}