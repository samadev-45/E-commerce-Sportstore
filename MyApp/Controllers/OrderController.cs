using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("user/create")]
        public async Task<IActionResult> CreateOrder()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            var order = await _orderService.CreateOrderAsync(userId);

            return this.OkResponse(order, "Order created successfully");
        }

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

        [HttpPut("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            await _orderService.CancelOrderAsync(orderId);
            return this.OkResponse<object>(null, "Order cancelled successfully");
        }
    }
}
