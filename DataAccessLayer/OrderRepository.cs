using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.OrderId == id);
        }
    }
}