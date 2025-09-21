using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Helpers;
using MyApp.Services.Interfaces;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // -----------------------------
        // Get cart items for current user
        // -----------------------------
        [HttpGet("user")]
        public async Task<IActionResult> GetCartForCurrentUser()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            var cart = await _cartService.GetUserCartAsync(userId);

            return this.OkResponse(cart, "Cart retrieved successfully");
        }

        // -----------------------------
        // Add product to cart
        // -----------------------------
        [HttpPost("user/add/{productId}")]
        public async Task<IActionResult> AddToCart(int productId, [FromQuery] int quantity = 1)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            await _cartService.AddToCartAsync(userId, productId, quantity);

            return this.OkResponse<object>(null, "Product added to cart");
        }

        // -----------------------------
        // Remove product from cart
        // -----------------------------
        [HttpDelete("user/remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            await _cartService.RemoveFromCartAsync(userId, productId);

            return this.OkResponse<object>(null, "Product removed from cart");
        }

        // -----------------------------
        // Update quantity of a product
        // -----------------------------
        [HttpPut("user/update/{productId}")]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromQuery] int quantity)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            await _cartService.UpdateQuantityAsync(userId, productId, quantity);

            return this.OkResponse<object>(null, "Quantity updated");
        }
    }
}
