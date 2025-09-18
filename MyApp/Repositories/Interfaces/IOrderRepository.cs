using MyApp.Entities;

namespace MyApp.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserAsync(int userId);
        Task SaveChangesAsync();
    }
}
