using MyApp.DTOs.Orders;
using MyApp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp.Services.Interfaces
{
    public interface IOrderService : IGenericService<Order, OrderDto>
    {
        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);
        Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId);
        Task CancelOrderAsync(int orderId);
        Task<IEnumerable<AdminOrderDto>> GetAllOrdersForAdminAsync(string? search = null, string? status = null);
        Task<string> UpdateOrderStatusAsync(
        int orderId,string? status = null,int? statusId = null,string? modifiedByUserId = null);



        // Test mode online payment
        Task<object> PayOnlineAsync(int orderId);
    }
}
