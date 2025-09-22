using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs.Orders;
using MyApp.Helpers;
using MyApp.Services.Interfaces;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // -----------------------------
        // Create order for current user
        // -----------------------------
        [HttpPost("user/create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);

            var order = await _orderService.CreateOrderAsync(userId, dto);

            return this.OkResponse(order, "Order created successfully");
        }

        // -----------------------------
        // Get all orders for current user
        // -----------------------------
        [HttpGet("user")]
        public async Task<IActionResult> GetOrdersForCurrentUser()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            var orders = await _orderService.GetOrdersByUserAsync(userId);

            return this.OkResponse(orders, "Orders retrieved successfully");
        }

        // -----------------------------
        // Cancel order
        // -----------------------------
        [HttpPut("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            await _orderService.CancelOrderAsync(orderId);
            return this.OkResponse<object?>(null, "Order cancelled successfully");
        }
    }
}
