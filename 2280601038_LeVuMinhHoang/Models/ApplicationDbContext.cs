using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _2280601038_LeVuMinhHoang.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalPrice)
                      .HasColumnType("decimal(18,2)");

                modelBuilder.Entity<OrderDetail>(entity =>
                {
                    entity.Property(e => e.Price)
                          .HasColumnType("decimal(18,2)");
                });

                modelBuilder.Entity<Product>(entity =>
                {
                    entity.Property(e => e.Price)
                          .HasColumnType("decimal(18,2)");
                });
            });
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
