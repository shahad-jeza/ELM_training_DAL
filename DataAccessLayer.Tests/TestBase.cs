using Moq;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer;
using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Tests
{
    public abstract class TestBase
    {
        protected Mock<AppDbContext> CreateMockDbContext<T>(List<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.AsQueryable().GetEnumerator());

            var mockContext = new Mock<AppDbContext>();
            mockContext.Setup(c => c.Set<T>()).Returns(mockSet.Object);

            return mockContext;
        }

        protected List<User> GetTestUsers()
        {
            return new List<User>
            {
                new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            };
        }

        protected List<Order> GetTestOrders()
        {
            return new List<Order>
            {
                new Order { OrderId = 1, UserId = 1, Product = "Laptop", Quantity = 1, Price = 999.99m },
                new Order { OrderId = 2, UserId = 1, Product = "Mouse", Quantity = 2, Price = 25.50m },
                new Order { OrderId = 3, UserId = 2, Product = "Keyboard", Quantity = 1, Price = 75.00m }
            };
        }
    }
}