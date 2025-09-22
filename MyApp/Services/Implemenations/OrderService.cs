using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Orders;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MyApp.Services.Implementations
{
    public class OrderService : GenericService<Order, OrderDto>, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
        ) : base(orderRepository, mapper, httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        // -----------------------------
        // USER SIDE
        // -----------------------------

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            var cartItems = await _cartRepository.GetCartByUserIdAsync(userId);
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            var order = new Order
            {
                UserId = userId,
                Address = dto.Address,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                OrderItems = new List<OrderItem>()
            };

            decimal total = 0;

            foreach (var cartItem in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product {cartItem.ProductId} not found.");

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price,
                    Price = product.Price * cartItem.Quantity
                };

                order.OrderItems.Add(orderItem);
                total += orderItem.Price;
            }

            order.TotalPrice = total;

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Clear cart
            foreach (var item in cartItems)
                await _cartRepository.DeleteAsync(item);

            await _cartRepository.SaveChangesAsync();

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            order.Status = "Cancelled";
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
        }

        // -----------------------------
        // ADMIN SIDE
        // -----------------------------

        public async Task<IEnumerable<AdminOrderDto>> GetAllOrdersForAdminAsync(string? search, string? status)
        {
            var query = _orderRepository.Query()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Search by OrderId or UserName
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.Id.ToString().Contains(search) ||
                    o.User.Name.Contains(search));
            }

            //  Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var orders = await query.ToListAsync();
            return _mapper.Map<IEnumerable<AdminOrderDto>>(orders);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            order.Status = status;
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
        }
    }
}
