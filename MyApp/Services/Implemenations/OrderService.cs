using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.DTOs.Orders;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using MyApp.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services.Implementations
{
    public class OrderService : GenericService<Order, OrderDto>, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRazorpayService _razorpayService;
        private readonly AppDbContext _context; 

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
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
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Search by user name or order ID
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(o =>
                    o.User.Name.ToLower().Contains(search) ||
                    o.Id.ToString() == search
                );
            }

            // Filter by status (trim and lower for safety)
            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim().ToLower();
                query = query.Where(o => (o.Status ?? "").Trim().ToLower() == status);
            }

            var orders = await query.ToListAsync();

            // Map manually to ensure nested items are included correctly
            var result = orders.Select(o => new AdminOrderDto
            {
                OrderId = o.Id.ToString(),
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
                    // Don't assign TotalPrice; it's calculated automatically
                }).ToList()

            });

            return result;
        }


        // -----------------------------
        // Admin: Get order by ID
        // -----------------------------
        public async Task<AdminOrderDto?> GetOrderByIdForAdminAsync(int orderId)
        {
            // Include User and OrderItems with Product
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            // Map to AdminOrderDto manually
            var result = new AdminOrderDto
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

            return result;
        }



        // -----------------------------
        // Create order for user
        // -----------------------------
        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItems = await _cartRepository.GetCartByUserIdAsync(userId);
                if (!cartItems.Any())
                    throw new InvalidOperationException("Cart is empty.");

                var order = _mapper.Map<Order>(dto);
                order.UserId = userId;
                order.OrderItems = new List<OrderItem>();
                decimal total = 0;

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

                await _orderRepository.AddAsync(order);

                // Remove cart items
                await Task.WhenAll(cartItems.Select(c => _cartRepository.DeleteAsync(c)));

                await _orderRepository.SaveChangesAsync();

                if (dto.PaymentMethod == PaymentMethod.Online)
                {
                    await HandleOnlinePayment(order);
                }

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
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
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
            {
                order.Status = status;
            }
            else if (statusId.HasValue)
            {
                order.Status = statusId.Value switch
                {
                    1 => OrderStatus.Pending.ToString(),
                    2 => OrderStatus.Shipped.ToString(),
                    3 => OrderStatus.Delivered.ToString(),
                    _ => throw new ArgumentException("Invalid statusId.")
                };
            }

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
