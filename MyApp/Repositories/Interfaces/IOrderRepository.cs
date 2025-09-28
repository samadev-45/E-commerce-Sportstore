using MyApp.Entities;

namespace MyApp.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetAllOrdersForAdminAsync(string? search = null, string? status = null);
        Task<Order?> GetOrderByIdForAdminAsync(int orderId);
    }
}
