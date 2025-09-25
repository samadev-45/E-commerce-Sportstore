using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Orders;
using MyApp.Services.Interfaces;
using System.Security.Claims;

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
        // Get all orders (search & filter)
        // -----------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status)
        {
            var orders = await _orderService.GetAllOrdersForAdminAsync(search, status);
            var response = new ApiResponse<IEnumerable<AdminOrderDto>>(orders, true, 200, "Orders retrieved successfully");
            return Ok(response);
        }

        // -----------------------------
        // Get a specific order by ID
        // -----------------------------
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetById(int orderId)
        {
            var orders = await _orderService.GetAllOrdersForAdminAsync(null, null);
            var order = orders.FirstOrDefault(o => o.OrderId == $"order_{orderId}");


            if (order == null)
                return NotFound(new ApiResponse<string>(
                    null,
                    false,
                    404,
                    "Order not found"
                ));

            return Ok(new ApiResponse<AdminOrderDto>(
                order,
                true,
                200,
                "Order retrieved successfully"
            ));
        }

        // -----------------------------
        // Mark order as delivered
        // -----------------------------
        [HttpPut("{orderId}/deliver")]
        public async Task<IActionResult> MarkDelivered(int orderId)
        {
            var status = await _orderService.UpdateOrderStatusAsync(
                orderId: orderId,
                status: null,
                statusId: 3, // Delivered
                modifiedByUserId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            );

            return Ok(new ApiResponse<string>(
                status,
                true,
                200,
                $"Order marked as {status}"
            ));
        }

        // -----------------------------
        // Update order status (Pending, Shipped, Delivered)
        // -----------------------------
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] AdminOrderStatusUpdateDto dto)
        {
            // Validate numeric status
            if (dto.Status < 1 || dto.Status > 3)
                return BadRequest(new ApiResponse<string>(
                    null,
                    false,
                    400,
                    "Invalid status. Must be 1 (Pending), 2 (Shipped), or 3 (Delivered)"
                ));

            var status = await _orderService.UpdateOrderStatusAsync(
                orderId: orderId,
                status: null,
                statusId: dto.Status,
                modifiedByUserId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            );

            return Ok(new ApiResponse<string>(
                status,
                true,
                200,
                $"Order status updated to {status}"
            ));
        }
    }
}
