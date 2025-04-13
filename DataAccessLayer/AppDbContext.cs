using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Models;

namespace DataAccessLayer
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial data for testing
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order { OrderId = 1, UserId = 1, Product = "Laptop", Quantity = 1, Price = 999.99m },
                new Order { OrderId = 2, UserId = 1, Product = "Mouse", Quantity = 2, Price = 25.50m },
                new Order { OrderId = 3, UserId = 2, Product = "Keyboard", Quantity = 1, Price = 75.00m }
            );
        }
    }
}