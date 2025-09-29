using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Wishlist;
using MyApp.Services.Interfaces;
using System;
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

        // GET: api/wishlist/user
        [HttpGet("user")]
        public async Task<IActionResult> GetWishlistForCurrentUser()
        {
            var wishlist = await _wishlistService.GetUserWishlistAsync(GetUserId());
            return Ok(ApiResponse.SuccessResponse(wishlist, "Wishlist retrieved successfully"));
        }

        // POST: api/wishlist/user/toggle/{productId}
        [HttpPost("user/toggle/{productId}")]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var userId = GetUserId();
            var result = await _wishlistService.ToggleWishlistAsync(userId, productId);

            if (result == null)
                return Ok(ApiResponse.SuccessResponse(null, "Product removed from wishlist"));

            return Ok(ApiResponse.SuccessResponse(result, "Product added to wishlist"));
        }

        // POST: api/wishlist/user/move-to-cart/{productId}
        [HttpPost("user/move-to-cart/{productId}")]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            await _wishlistService.MoveToCartAsync(GetUserId(), productId);
            var wishlist = await _wishlistService.GetUserWishlistAsync(GetUserId());
            return Ok(ApiResponse.SuccessResponse(wishlist, "Product moved to cart"));
        }
    }
}
