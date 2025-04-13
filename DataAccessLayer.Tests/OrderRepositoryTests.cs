using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DataAccessLayer.Repositories;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Tests
{
    [TestFixture]
    public class OrderRepositoryTests : TestBase
    {
        private Mock<AppDbContext> _mockContext;
        private OrderRepository _repository;
        private List<Order> _orders;
        private List<User> _users;

        [SetUp]
        public void Setup()
        {
            _users = GetTestUsers();
            _orders = GetTestOrders();

            // Setup Orders DbSet
            var mockOrderSet = new Mock<DbSet<Order>>();
            mockOrderSet.As<IAsyncEnumerable<Order>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Order>(_orders.GetEnumerator()));

            mockOrderSet.As<IQueryable<Order>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Order>(_orders.AsQueryable().Provider));

            mockOrderSet.As<IQueryable<Order>>()
                .Setup(m => m.Expression)
                .Returns(_orders.AsQueryable().Expression);

            mockOrderSet.As<IQueryable<Order>>()
                .Setup(m => m.ElementType)
                .Returns(_orders.AsQueryable().ElementType);

            mockOrderSet.As<IQueryable<Order>>()
                .Setup(m => m.GetEnumerator())
                .Returns(_orders.AsQueryable().GetEnumerator());

            mockOrderSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => _orders.FirstOrDefault(o => o.OrderId == (int)ids[0]));

            // Setup Users DbSet for Include operations
            var mockUserSet = new Mock<DbSet<User>>();
            mockUserSet.As<IQueryable<User>>()
                .Setup(m => m.Provider)
                .Returns(_users.AsQueryable().Provider);

            mockUserSet.As<IQueryable<User>>()
                .Setup(m => m.Expression)
                .Returns(_users.AsQueryable().Expression);

            mockUserSet.As<IQueryable<User>>()
                .Setup(m => m.ElementType)
                .Returns(_users.AsQueryable().ElementType);

            mockUserSet.As<IQueryable<User>>()
                .Setup(m => m.GetEnumerator())
                .Returns(_users.AsQueryable().GetEnumerator());

            mockUserSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => _users.FirstOrDefault(u => u.Id == (int)ids[0]));

            _mockContext = new Mock<AppDbContext>();
            _mockContext.Setup(c => c.Orders).Returns(mockOrderSet.Object);
            _mockContext.Setup(c => c.Set<Order>()).Returns(mockOrderSet.Object);
            _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.Set<User>()).Returns(mockUserSet.Object);

            _repository = new OrderRepository(_mockContext.Object);
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsOrderWithUser()
        {
            var result = await _repository.GetByIdAsync(1);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("Laptop", result.Product);
            Assert.IsNotNull(result.User);
            Assert.AreEqual("John", result.User.FirstName);
        }

        [Test]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(99);
            
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllOrdersWithUsers()
        {
            var result = await _repository.GetAllAsync();
            
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.All(o => o.User != null));
        }

        [Test]
        public async Task GetByUserIdAsync_ExistingUserId_ReturnsUserOrders()
        {
            var result = await _repository.GetByUserIdAsync(1);
            
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(o => o.UserId == 1));
        }

        [Test]
        public async Task GetByUserIdAsync_NonExistingUserId_ReturnsEmptyList()
        {
            var result = await _repository.GetByUserIdAsync(99);
            
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task AddAsync_ValidOrder_AddsToDatabase()
        {
            var newOrder = new Order 
            { 
                OrderId = 4, 
                UserId = 1, 
                Product = "Monitor", 
                Quantity = 1, 
                Price = 199.99m 
            };
            
            await _repository.AddAsync(newOrder);
            
            _mockContext.Verify(m => m.Set<Order>().AddAsync(newOrder, It.IsAny<CancellationToken>()), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void AddAsync_NullOrder_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null));
        }

        [Test]
        public void AddAsync_InvalidOrder_ThrowsException()
        {
            var invalidOrder = new Order { OrderId = 5, UserId = 99 }; // Missing required fields
            
            Assert.ThrowsAsync<DbUpdateException>(() => _repository.AddAsync(invalidOrder));
        }

        [Test]
        public async Task UpdateAsync_ExistingOrder_UpdatesDatabase()
        {
            var order = _orders.First();
            order.Product = "Updated Product";
            
            await _repository.UpdateAsync(order);
            
            _mockContext.Verify(m => m.Set<Order>().Update(order), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void UpdateAsync_NullOrder_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null));
        }

        [Test]
        public async Task DeleteAsync_ExistingId_RemovesFromDatabase()
        {
            await _repository.DeleteAsync(1);
            
            _mockContext.Verify(m => m.Set<Order>().Remove(It.Is<Order>(o => o.OrderId == 1)), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_NonExistingId_DoesNothing()
        {
            await _repository.DeleteAsync(99);
            
            _mockContext.Verify(m => m.Set<Order>().Remove(It.IsAny<Order>()), Times.Never);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            var result = await _repository.ExistsAsync(1);
            
            Assert.IsTrue(result);
        }

        [Test]
        public async Task ExistsAsync_NonExistingId_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync(99);
            
            Assert.IsFalse(result);
        }
    }
}