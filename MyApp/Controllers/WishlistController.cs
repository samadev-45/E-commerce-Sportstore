using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Helpers;
using MyApp.Services.Interfaces;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetWishlistForCurrentUser()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            var wishlist = await _wishlistService.GetUserWishlistAsync(userId);

            return this.OkResponse(wishlist, "Wishlist retrieved successfully");
        }

        [HttpPost("user/add/{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            await _wishlistService.AddToWishlistAsync(userId, productId);

            return this.OkResponse<object?>(null, "Product added to wishlist");
        }

        [HttpDelete("user/remove/{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            await _wishlistService.RemoveFromWishlistAsync(userId, productId);

            return this.OkResponse<object?>(null, "Product removed from wishlist");
        }

        [HttpPost("user/move-to-cart/{productId}")]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return this.BadResponse("Unauthorized", 401);

            int userId = int.Parse(userIdClaim.Value);
            await _wishlistService.MoveToCartAsync(userId, productId);

            return this.OkResponse<object?>(null, "Product moved to cart");
        }
    }
}
