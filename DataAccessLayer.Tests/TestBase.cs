using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer;
using System.Data.Common;

namespace DataAccessLayer.Tests
{
    public abstract class TestBase : IDisposable
    {
        private readonly DbConnection _connection;
        protected readonly AppDbContext _context;

        protected TestBase()
        {
            // Create and open a SQLite connection
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            // Configure the context to use SQLite
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            
            // Ensure the database is created
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        protected void SeedDatabase()
        {
            // Add test users
            _context.Users.AddRange(
                new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            );

            // Add test orders
            _context.Orders.AddRange(
                new Order { OrderId = 1, UserId = 1, Product = "Laptop", Quantity = 1, Price = 999.99m },
                new Order { OrderId = 2, UserId = 1, Product = "Mouse", Quantity = 2, Price = 25.50m },
                new Order { OrderId = 3, UserId = 2, Product = "Keyboard", Quantity = 1, Price = 75.00m }
            );

            _context.SaveChanges();
        }
    }
}