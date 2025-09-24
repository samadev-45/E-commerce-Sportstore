using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs.Orders;
using MyApp.Services.Interfaces;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public PaymentController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("razorpay")]
        public async Task<IActionResult> PayOrder([FromBody] PaymentDto dto)
        {
            var result = await _orderService.PayOnlineAsync(dto.OrderId);
            return Ok(result);
        }
    }
}
