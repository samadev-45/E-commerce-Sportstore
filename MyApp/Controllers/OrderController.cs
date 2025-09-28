using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Orders;
using MyApp.Helpers;
using MyApp.Services.Interfaces;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
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
                return Unauthorized(ApiResponse.FailResponse("Unauthorized"));

            int userId = int.Parse(userIdClaim.Value);

            // Only send address & paymentType; items come from cart
            var order = await _orderService.CreateOrderAsync(userId, dto);

            return Ok(ApiResponse.SuccessResponse(order, "Order created successfully"));
        }

        // -----------------------------
        // Get all orders for current user
        // -----------------------------
        [HttpGet("user")]
        public async Task<IActionResult> GetOrdersForCurrentUser()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return Unauthorized(ApiResponse.FailResponse("Unauthorized"));

            int userId = int.Parse(userIdClaim.Value);
            var orders = await _orderService.GetOrdersByUserAsync(userId);

            return Ok(ApiResponse.SuccessResponse(orders, "Orders retrieved successfully"));
        }

        // -----------------------------
        // Cancel order
        // -----------------------------
        [HttpPut("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            await _orderService.CancelOrderAsync(orderId);
            return Ok(ApiResponse.SuccessResponse(null, "Order cancelled successfully"));
        }
    }
}
