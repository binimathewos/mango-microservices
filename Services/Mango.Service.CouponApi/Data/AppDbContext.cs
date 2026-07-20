using Microsoft.EntityFrameworkCore;
using Mango.Service.CouponApi.Models;

namespace Mango.Service.CouponApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Coupon>().HasData(
                new Coupon
                {
                    CouponId = 1,
                    CouponCode = "10OFF",
                    DiscountAmount = 10.0,
                    MinimumAmount = 20.0
                },
                new Coupon
                {
                    CouponId = 2,
                    CouponCode = "20OFF",
                    DiscountAmount = 20.0,
                    MinimumAmount = 40.0
                }
            );
        }
    }
}