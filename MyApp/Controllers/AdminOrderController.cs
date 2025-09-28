using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Orders;
using MyApp.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyApp.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")] // Only admins
    public class AdminOrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminOrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // -----------------------------
        // Helper: Get current admin id
        // -----------------------------
        private string? GetAdminId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // -----------------------------
        // Get all orders (search & filter)
        // -----------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status)
        {
            var orders = await _orderService.GetAllOrdersForAdminAsync(search, status);
            return Ok(ApiResponse.SuccessResponse(orders, "Orders retrieved successfully"));
        }

        // -----------------------------
        // Get a specific order by ID
        // -----------------------------
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetById(int orderId)
        {
            var order = await _orderService.GetOrderByIdForAdminAsync(orderId);
            if (order == null)
                return NotFound(ApiResponse.FailResponse("Order not found", 404));

            return Ok(ApiResponse.SuccessResponse(order, "Order retrieved successfully"));
        }

        // -----------------------------
        // Mark order as delivered
        // -----------------------------
        [HttpPut("{orderId}/deliver")]
        public async Task<IActionResult> MarkDelivered(int orderId)
        {
            var adminId = GetAdminId();

            var status = await _orderService.UpdateOrderStatusAsync(
                orderId: orderId,
                status: null,
                statusId: 3, // Delivered
                modifiedByUserId: adminId
            );

            return Ok(ApiResponse.SuccessResponse(status, $"Order marked as {status}"));
        }

        // -----------------------------
        // Update order status (Pending, Shipped, Delivered)
        // -----------------------------
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] AdminOrderStatusUpdateDto dto)
        {
            var adminId = GetAdminId();

            // Validate numeric status
            if (dto.Status < 1 || dto.Status > 3)
                return BadRequest(ApiResponse.FailResponse("Invalid status. Must be 1 (Pending), 2 (Shipped), or 3 (Delivered)"));

            var status = await _orderService.UpdateOrderStatusAsync(
                orderId: orderId,
                status: null,
                statusId: dto.Status,
                modifiedByUserId: adminId
            );

            return Ok(ApiResponse.SuccessResponse(status, $"Order status updated to {status}"));
        }
    }
}
