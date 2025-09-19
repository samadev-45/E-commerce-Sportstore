using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs.Orders;
using MyApp.Services.Interfaces;
using System.Security.Claims;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Only logged-in users can order
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Place a new order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _orderService.CreateOrderAsync(userId, dto);

            return CreatedAtAction(nameof(GetOrdersByUser), new { userId = userId }, order);
        }

        // Get orders of logged-in user
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetOrdersByUser()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var orders = await _orderService.GetOrdersByUserAsync(userId);

            return Ok(orders);
        }

        //Cancel order
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _orderService.CancelOrderAsync(userId, id);

            if (!result)
                return BadRequest("Order cannot be cancelled or not found");

            return Ok(new { message = "Order cancelled successfully" });
        }

    }
}
