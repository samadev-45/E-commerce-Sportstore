using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.DTOs.Orders;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppOrder = MyApp.Entities.Order;

namespace MyApp.Services.Implementations
{
    public enum OrderStatusEnum
    {
        Pending = 1,
        Shipped = 2,
        Delivered = 3,
        PaymentInitiated = 4,
        Cancelled = 5
    }

    public static class OrderStatusHelper
    {
        public static string ToString(OrderStatusEnum status) => status switch
        {
            OrderStatusEnum.Pending => "Pending",
            OrderStatusEnum.Shipped => "Shipped",
            OrderStatusEnum.Delivered => "Delivered",
            OrderStatusEnum.PaymentInitiated => "Payment Initiated",
            OrderStatusEnum.Cancelled => "Cancelled",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public class OrderService : GenericService<AppOrder, OrderDto>, IOrderService
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

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItems = await _cartRepository.GetCartByUserIdAsync(userId);
                if (!cartItems.Any())
                    throw new InvalidOperationException("Cart is empty.");

                var order = _mapper.Map<AppOrder>(dto);
                order.UserId = userId;
                order.OrderItems = new List<OrderItem>();
                decimal total = 0;
                //loop through cartItem
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
                order.Status = dto.PaymentType == "COD"
                    ? OrderStatusHelper.ToString(OrderStatusEnum.Pending)
                    : OrderStatusHelper.ToString(OrderStatusEnum.PaymentInitiated);

                await _orderRepository.AddAsync(order);

                // Remove cart items in parallel
                await Task.WhenAll(cartItems.Select(c => _cartRepository.DeleteAsync(c)));

                await _orderRepository.SaveChangesAsync();

                if (dto.PaymentType == "Online")
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

        private async Task HandleOnlinePayment(AppOrder order)
        {
            if (order.TotalPrice < 1)
                throw new InvalidOperationException("Order total must be at least ₹1.");

            var razorpayOrder = _razorpayService.CreateOrder(
                (int)(order.TotalPrice * 100), "INR", $"order_{order.Id}");

            order.PaymentId = razorpayOrder["id"].ToString();
            order.Status = OrderStatusHelper.ToString(OrderStatusEnum.PaymentInitiated);

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

            order.Status = OrderStatusHelper.ToString(OrderStatusEnum.Cancelled);
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<AdminOrderDto>> GetAllOrdersForAdminAsync(string? search, string? status)
        {
            var query = _orderRepository.Query()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(o => o.Id.ToString().Contains(search) || o.User.Name.Contains(search));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            var orders = await query.ToListAsync();
            return _mapper.Map<IEnumerable<AdminOrderDto>>(orders);
        }

        public async Task<string> UpdateOrderStatusAsync(int orderId, string? status = null, int? statusId = null, string? modifiedByUserId = null)
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
                    1 => OrderStatusHelper.ToString(OrderStatusEnum.Pending),
                    2 => OrderStatusHelper.ToString(OrderStatusEnum.Shipped),
                    3 => OrderStatusHelper.ToString(OrderStatusEnum.Delivered),
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

        //Useful when the user decides to pay later for an already created order
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
