using MyApp.DTOs.Orders;
using MyApp.Entities;
using MyApp.Services.Interfaces;

public interface IOrderService : IGenericService<Order, OrderDto>
{
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
    Task CancelOrderAsync(int orderId);
    Task<IEnumerable<AdminOrderDto>> GetAllOrdersForAdminAsync(string? search = null, string? status = null);
    Task UpdateOrderStatusAsync(int orderId, string status);

}
