using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace DataAccessLayer.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private AppDbContext _context;
        private UserRepository _userRepository;
        private OrderRepository _orderRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _userRepository = new UserRepository(_context);
            _orderRepository = new OrderRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task UserRepository_AddAsync_ShouldPersistUser()
        {
            var newUser = new User 
            { 
                Id = 10, 
                FirstName = "Integration", 
                LastName = "Test", 
                Email = "integration@test.com" 
            };

            await _userRepository.AddAsync(newUser);
            
            var result = await _userRepository.GetByIdAsync(10);
            Assert.IsNotNull(result);
            Assert.AreEqual("Integration", result.FirstName);
        }

        [Test]
        public async Task OrderRepository_GetByUserIdAsync_ShouldReturnUserOrders()
        {
            // Seed test data
            await _userRepository.AddAsync(new User { Id = 100, FirstName = "Test", LastName = "User", Email = "test@user.com" });
            await _orderRepository.AddAsync(new Order { OrderId = 100, UserId = 100, Product = "Test Product", Quantity = 1, Price = 10.00m });
            await _orderRepository.AddAsync(new Order { OrderId = 101, UserId = 100, Product = "Another Product", Quantity = 2, Price = 20.00m });

            var result = await _orderRepository.GetByUserIdAsync(100);
            
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task OrderRepository_DeleteAsync_ShouldRemoveOrder()
        {
            // Seed test data
            await _userRepository.AddAsync(new User { Id = 200, FirstName = "Delete", LastName = "Test", Email = "delete@test.com" });
            await _orderRepository.AddAsync(new Order { OrderId = 200, UserId = 200, Product = "To Delete", Quantity = 1, Price = 15.00m });

            await _orderRepository.DeleteAsync(200);
            
            var result = await _orderRepository.GetByIdAsync(200);
            Assert.IsNull(result);
        }
    }
}