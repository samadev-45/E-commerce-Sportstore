using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Orders;
using MyApp.Services.Interfaces;

namespace MyApp.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")] 
    public class AdminOrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminOrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // -----------------------------
        // Get all orders (with search & filter)
        // -----------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            var orders = await _orderService.GetAllOrdersForAdminAsync(search, status);
            var response = new ApiResponse<IEnumerable<AdminOrderDto>>(orders, true, 200, "Orders retrieved successfully");
            return Ok(response);
        }

        // -----------------------------
        // Mark order as delivered
        // -----------------------------
        [HttpPut("{orderId}/deliver")]
        public async Task<IActionResult> MarkDelivered(int orderId)
        {
            await _orderService.UpdateOrderStatusAsync(orderId, "Delivered");
            var response = new ApiResponse<object?>(null, true, 200, "Order marked as delivered");
            return Ok(response);
        }

        // -----------------------------
        // Update order status (Pending, Shipped, Delivered, etc.)
        // -----------------------------
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromQuery] string status)
        {
            await _orderService.UpdateOrderStatusAsync(orderId, status);
            var response = new ApiResponse<object?>(null, true, 200, $"Order status updated to {status}");
            return Ok(response);
        }
    }
}
