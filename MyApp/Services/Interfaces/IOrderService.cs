using MyApp.DTOs.Orders;
using MyApp.Entities;

namespace MyApp.Services.Interfaces
{
    public interface IOrderService : IGenericService<Order, OrderDto>
    {
        Task<OrderDto> CreateOrderAsync(int userId);
        Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
        Task CancelOrderAsync(int orderId);
    }
}
