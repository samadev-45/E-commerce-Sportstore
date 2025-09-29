using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Cart;
using MyApp.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            return int.Parse(userIdClaim.Value);
        }

        // -----------------------------
        // Get cart items for current user
        // -----------------------------
        [HttpGet("user")]
        public async Task<IActionResult> GetUserCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(ApiResponse.SuccessResponse(cart, "Cart retrieved successfully"));
        }

        // -----------------------------
        // Add product to cart
        // -----------------------------
        [HttpPost("user/add/{productId}")]
        public async Task<IActionResult> AddToCart(int productId, [FromQuery] int quantity = 1)
        {
            var userId = GetUserId();
            var cartItem = await _cartService.AddToCartAsync(userId, productId, quantity);

            return Ok(ApiResponse.SuccessResponse(cartItem, "Product added to cart"));
        }

        // -----------------------------
        // Remove product from cart
        // -----------------------------
        [HttpDelete("user/remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = GetUserId();
            await _cartService.RemoveFromCartAsync(userId, productId);

            return Ok(ApiResponse.SuccessResponse(null, "Product removed from cart"));
        }

        // -----------------------------
        // Update quantity of a product
        // -----------------------------
        [HttpPut("user/update/{productId}")]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromQuery] int quantity)
        {
            var userId = GetUserId();
            var cartItem = await _cartService.UpdateQuantityAsync(userId, productId, quantity);

            if (cartItem == null)
                return NotFound(ApiResponse.SuccessResponse(null, "Product not found in cart"));

            return Ok(ApiResponse.SuccessResponse(cartItem, "Quantity updated"));
        }
    }
}
