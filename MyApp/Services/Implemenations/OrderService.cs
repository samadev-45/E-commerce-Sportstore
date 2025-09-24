using AppOrder = MyApp.Entities.Order;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Orders;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using MyApp.Services.Interfaces;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services.Implementations
{
    public class OrderService : GenericService<AppOrder, OrderDto>, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRazorpayService _razorpayService;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IRazorpayService razorpayService
        ) : base(orderRepository, mapper, httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _razorpayService = razorpayService;
        }

        // -----------------------------
        // Create order for a user
        // -----------------------------
        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            var cartItems = await _cartRepository.GetCartByUserIdAsync(userId);
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            var order = _mapper.Map<AppOrder>(dto);
            order.UserId = userId;
            order.OrderItems = new List<OrderItem>();

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
            order.Status = dto.PaymentType == "COD" ? "Pending" : "Payment Initiated";

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Clear user's cart
            foreach (var item in cartItems)
                await _cartRepository.DeleteAsync(item);
            await _cartRepository.SaveChangesAsync();

            // -----------------------------
            // Online payment via Razorpay
            // -----------------------------
            //if (dto.PaymentType == "Online")
            //{
            //    int amountInPaise = (int)(total * 100); // INR to paise
            //    var razorpayOrder = _razorpayService.CreateOrder(amountInPaise, "INR", $"order_{order.Id}");
            //    order.PaymentId = razorpayOrder["id"].ToString();
            //    order.Status = "Payment Initiated";

            //    await _orderRepository.UpdateAsync(order);
            //    await _orderRepository.SaveChangesAsync();
            //}
            if (dto.PaymentType == "Online")
            {
                 total = order.TotalPrice;
                if (total < 1)
                    throw new InvalidOperationException("Total order amount must be at least ₹1 to create a Razorpay order.");

                int amountInPaise = (int)(total * 100); // convert INR to paise

                try
                {
                    // Create Razorpay test order
                    var razorpayOrder = _razorpayService.CreateOrder(
                        amountInPaise,
                        "INR",
                        "receipt_" + order.Id // optional: use order ID as receipt
                    );

                    // Store Razorpay order ID
                    order.PaymentId = razorpayOrder["id"].ToString();
                    order.Status = "Payment Initiated";

                    await _orderRepository.UpdateAsync(order);
                    await _orderRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Handle Razorpay errors
                    throw new InvalidOperationException("Razorpay order creation failed: " + ex.Message);
                }
            }


            return _mapper.Map<OrderDto>(order);
        }

        // -----------------------------
        // Get orders for a user
        // -----------------------------
        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        // -----------------------------
        // Cancel an order
        // -----------------------------
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
        // Admin methods
        // -----------------------------
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

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found.");

            order.Status = status;
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task<string?> UpdateOrderStatusAsync(int orderId, int statusId, string? modifiedByUserId = null)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.IsDeleted) return null;

            string status = statusId switch
            {
                1 => "Pending",
                2 => "Shipped",
                3 => "Delivered",
                _ => throw new ArgumentException("Invalid statusId. Must be 1, 2, or 3.")
            };

            order.Status = status;
            order.ModifiedOn = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(modifiedByUserId) && int.TryParse(modifiedByUserId, out int modifiedBy))
                order.ModifiedBy = modifiedBy;

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
            return status;
        }
        public async Task<object> PayOnlineAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            int amountInPaise = (int)(order.TotalPrice * 100);

            // Create test Razorpay order
            var razorpayOrder = _razorpayService.CreateOrder(amountInPaise, "INR", $"order_{order.Id}");

            // Save Razorpay order id to order
            order.PaymentId = razorpayOrder["id"].ToString();
            order.Status = "Payment Initiated";

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return new
            {
                orderId = order.Id,
                razorpayOrderId = order.PaymentId,
                amount = order.TotalPrice
            };
        }


    }
}
