using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Orders;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;

namespace MyApp.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepo, ICartRepository cartRepo, IMapper mapper)
        {
            _orderRepo = orderRepo;
            _cartRepo = cartRepo;
            _mapper = mapper;
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            var cartItems = await _cartRepo.GetUserCartAsync(userId);  
            if (!cartItems.Any())
                throw new Exception("Cart is empty. Add products before checkout.");

            var order = new Order
            {
                UserId = userId,
                Address = dto.Address,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Product.Price
                }).ToList()
            };

            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();

            await _cartRepo.ClearCartAsync(userId); 

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _orderRepo.GetByUserAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<bool> CancelOrderAsync(int userId, int orderId)
        {
            var order = await _orderRepo.GetByUserAndIdAsync(userId, orderId);

            if (order == null || order.Status != "Pending")
                return false;

            order.Status = "Cancelled";
            await _orderRepo.SaveChangesAsync();

            return true;
        }




    }
}
