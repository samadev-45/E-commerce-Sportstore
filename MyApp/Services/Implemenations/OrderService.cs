using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyApp.Common.Enums;
using MyApp.Data;
using MyApp.DTOs.Orders;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services.Implementations
{
    public class OrderService : GenericService<Order, OrderDto>, IOrderService
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<CartItem> _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRazorpayService _razorpayService;
        private readonly AppDbContext _context;


        public OrderService(
            IGenericRepository<Order> orderRepository,
            IGenericRepository<CartItem> cartRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IRazorpayService razorpayService,
            AppDbContext context
        ) : base(orderRepository, mapper, httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _razorpayService = razorpayService;
            _context = context;
        }

        // -----------------------------
        // Admin: Get all orders (with search & filter)
        // -----------------------------
        public async Task<IEnumerable<AdminOrderDto>> GetAllOrdersForAdminAsync(string? search = null, string? status = null)
        {
            var query = _orderRepository.Query()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(o =>
                    o.User.Name.ToLower().Contains(search) ||
                    o.Id.ToString() == search
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim().ToLower();
                query = query.Where(o => (o.Status ?? "").Trim().ToLower() == status);
            }

            var orders = await query.ToListAsync();

            return orders.Select(o => new AdminOrderDto
            {
                OrderId = $"order_{o.Id}",
                UserName = o.User?.Name ?? string.Empty,
                UserEmail = o.User?.Email ?? string.Empty,
                Address = o.Address,
                Time = o.CreatedOn,
                Status = o.Status ?? "Pending",
                Items = o.OrderItems.Select(oi => new AdminOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            });
        }

        // -----------------------------
        // Admin: Get order by ID
        // -----------------------------
        public async Task<AdminOrderDto?> GetOrderByIdForAdminAsync(int orderId)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            return new AdminOrderDto
            {
                OrderId = $"order_{order.Id}",
                UserName = order.User.Name,
                UserEmail = order.User.Email,
                Address = order.Address,
                Time = order.OrderDate,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new AdminOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }

        // -----------------------------
        // Create order for user
        // -----------------------------
        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            // Start a database transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get cart items for the user, include product details
                var cartItems = await _cartRepository.Query()
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Product)
                    .ToListAsync();

                if (!cartItems.Any())
                    throw new InvalidOperationException("Cart is empty.");

                // Map order DTO to entity
                var order = _mapper.Map<Order>(dto);
                order.UserId = userId;
                order.OrderItems = new List<OrderItem>();
                decimal total = 0;

                // Create order items from cart
                foreach (var cartItem in cartItems)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId)
                        ?? throw new InvalidOperationException($"Product {cartItem.ProductId} not found.");

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
                order.Status = dto.PaymentMethod == PaymentMethod.COD
                    ? OrderStatus.Pending.ToString()
                    : OrderStatus.PaymentInitiated.ToString();

                // Add order to repository
                await _orderRepository.AddAsync(order);

                // Remove cart items
                foreach (var c in cartItems)
                    await _cartRepository.DeleteAsync(c);

                // Save all changes
                await _cartRepository.SaveChangesAsync();
                await _orderRepository.SaveChangesAsync();

                // Handle online payment if required
                if (dto.PaymentMethod == PaymentMethod.Online)
                    await HandleOnlinePayment(order);

                // Commit transaction
                await transaction.CommitAsync();

                return _mapper.Map<OrderDto>(order);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        private async Task HandleOnlinePayment(Order order)
        {
            if (order.TotalPrice < 1)
                throw new InvalidOperationException("Order total must be at least ₹1.");

            var razorpayOrder = _razorpayService.CreateOrder(
                (int)(order.TotalPrice * 100), "INR", $"order_{order.Id}");

            order.PaymentId = razorpayOrder["id"].ToString();
            order.Status = OrderStatus.PaymentInitiated.ToString();

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _orderRepository.Query()
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException("Order not found.");

            order.Status = OrderStatus.Cancelled.ToString();
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task<string> UpdateOrderStatusAsync(
            int orderId,
            string? status = null,
            int? statusId = null,
            string? modifiedByUserId = null)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.IsDeleted)
                throw new KeyNotFoundException("Order not found.");

            if (!string.IsNullOrEmpty(status))
                order.Status = status;
            else if (statusId.HasValue)
                order.Status = statusId.Value switch
                {
                    1 => OrderStatus.Pending.ToString(),
                    2 => OrderStatus.Shipped.ToString(),
                    3 => OrderStatus.Delivered.ToString(),
                    _ => throw new ArgumentException("Invalid statusId.")
                };

            order.ModifiedOn = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(modifiedByUserId) && int.TryParse(modifiedByUserId, out int modifiedBy))
                order.ModifiedBy = modifiedBy;

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order.Status;
        }

        public async Task<object> PayOnlineAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new KeyNotFoundException("Order not found.");

            await HandleOnlinePayment(order);

            return new
            {
                orderId = order.Id,
                razorpayOrderId = order.PaymentId,
                amount = order.TotalPrice
            };
        }
    }
}
