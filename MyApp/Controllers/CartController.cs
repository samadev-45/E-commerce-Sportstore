using MyApp.DTOs.Cart;
using MyApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // all cart actions require login
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartService.GetUserCartAsync(GetUserId());
            return Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var item = await _cartService.AddToCartAsync(GetUserId(), dto);
            return Ok(item);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var success = await _cartService.RemoveFromCartAsync(GetUserId(), productId);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] UpdateCartQuantityDto dto)
        {
            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be greater than 0");

            var updatedItem = await _cartService.UpdateQuantityAsync(GetUserId(), productId, dto.Quantity);

            if (updatedItem == null)
                return NotFound("Item not found in cart");

            return Ok(updatedItem);
        }

    }
}
