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
        // Mark order as delivered
        // -----------------------------
        [HttpPut("{orderId}/deliver")]
        public async Task<IActionResult> MarkDelivered(int orderId)
        {
            var status = await _orderService.UpdateOrderStatusAsync(
                orderId,
                3, // Delivered
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            );

            if (status == null) 
                return NotFound(new ApiResponse<string?>(
                    null,
                    false,
                    404,
                    "Order not found"
                ));

            return Ok(new ApiResponse<string?>(
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
                return BadRequest(new ApiResponse<string?>(
                    null,
                    false,
                    400,
                    "Invalid status. Must be 1 (Pending), 2 (Shipped), or 3 (Delivered)"
                ));

            var status = await _orderService.UpdateOrderStatusAsync(
                orderId,
                dto.Status,
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            );

            if (status == null)   
                return NotFound(new ApiResponse<string?>(
                    null,
                    false,
                    404,
                    "Order not found"
                ));

            return Ok(new ApiResponse<string?>(
                status,
                true,
                200,
                $"Order status updated to {status}"
            ));
        }


    }
}
