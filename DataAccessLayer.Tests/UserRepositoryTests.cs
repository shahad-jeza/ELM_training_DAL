using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DataAccessLayer.Repositories;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Tests
{
    [TestFixture]
    public class UserRepositoryTests : TestBase
    {
        private Mock<AppDbContext> _mockContext;
        private UserRepository _repository;
        private List<User> _users;

        [SetUp]
        public void Setup()
        {
            _users = GetTestUsers();
            _mockContext = CreateMockDbContext(_users);

            // Setup FindAsync for Users
            var mockSet = new Mock<DbSet<User>>();
            mockSet.As<IAsyncEnumerable<User>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<User>(_users.GetEnumerator()));

            mockSet.As<IQueryable<User>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<User>(_users.AsQueryable().Provider));

            mockSet.As<IQueryable<User>>()
                .Setup(m => m.Expression)
                .Returns(_users.AsQueryable().Expression);

            mockSet.As<IQueryable<User>>()
                .Setup(m => m.ElementType)
                .Returns(_users.AsQueryable().ElementType);

            mockSet.As<IQueryable<User>>()
                .Setup(m => m.GetEnumerator())
                .Returns(_users.AsQueryable().GetEnumerator());

            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => _users.FirstOrDefault(u => u.Id == (int)ids[0]));

            _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
            _mockContext.Setup(c => c.Set<User>()).Returns(mockSet.Object);

            _repository = new UserRepository(_mockContext.Object);
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsUser()
        {
            var result = await _repository.GetByIdAsync(1);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("John", result.FirstName);
        }

        [Test]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(99);
            
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            var result = await _repository.GetAllAsync();
            
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task AddAsync_ValidUser_AddsToDatabase()
        {
            var newUser = new User { Id = 3, FirstName = "New", LastName = "User", Email = "new@example.com" };
            
            await _repository.AddAsync(newUser);
            
            _mockContext.Verify(m => m.Set<User>().AddAsync(newUser, It.IsAny<CancellationToken>()), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void AddAsync_NullUser_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null));
        }

        [Test]
        public async Task UpdateAsync_ExistingUser_UpdatesDatabase()
        {
            var user = _users.First();
            user.FirstName = "Updated";
            
            await _repository.UpdateAsync(user);
            
            _mockContext.Verify(m => m.Set<User>().Update(user), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void UpdateAsync_NullUser_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null));
        }

        [Test]
        public async Task DeleteAsync_ExistingId_RemovesFromDatabase()
        {
            await _repository.DeleteAsync(1);
            
            _mockContext.Verify(m => m.Set<User>().Remove(It.Is<User>(u => u.Id == 1)), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_NonExistingId_DoesNothing()
        {
            await _repository.DeleteAsync(99);
            
            _mockContext.Verify(m => m.Set<User>().Remove(It.IsAny<User>()), Times.Never);
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