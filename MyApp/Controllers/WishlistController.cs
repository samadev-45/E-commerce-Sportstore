using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.Helpers;
using MyApp.Services.Interfaces;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID not found in token");

            return int.Parse(userIdClaim.Value);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetWishlistForCurrentUser()
        {
            var wishlist = await _wishlistService.GetUserWishlistAsync(GetUserId());
            return Ok(ApiResponse.SuccessResponse(wishlist, "Wishlist retrieved successfully"));
        }

        [HttpPost("user/add/{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            await _wishlistService.AddToWishlistAsync(GetUserId(), productId);
            var wishlist = await _wishlistService.GetUserWishlistAsync(GetUserId());
            return Ok(ApiResponse.SuccessResponse(wishlist, "Product added to wishlist"));
        }

        [HttpDelete("user/remove/{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            await _wishlistService.RemoveFromWishlistAsync(GetUserId(), productId);
            var wishlist = await _wishlistService.GetUserWishlistAsync(GetUserId());
            return Ok(ApiResponse.SuccessResponse(wishlist, "Product removed from wishlist"));
        }

        [HttpPost("user/move-to-cart/{productId}")]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            await _wishlistService.MoveToCartAsync(GetUserId(), productId);
            var wishlist = await _wishlistService.GetUserWishlistAsync(GetUserId());
            return Ok(ApiResponse.SuccessResponse(wishlist, "Product moved to cart"));
        }

    }
}
