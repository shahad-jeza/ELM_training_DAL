using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}