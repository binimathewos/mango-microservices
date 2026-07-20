using Microsoft.EntityFrameworkCore;
using Mango.Service.EmailApi.Models;

namespace Mango.Service.EmailApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<EmailLogger> EmailLoggers { get; set; }
    }
}