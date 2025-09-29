using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Wishlist;
using MyApp.DTOs.Cart;
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

        [HttpGet("user")]
        public async Task<IActionResult> GetWishlistForCurrentUser()
        {
            var wishlist = await _wishlistService.GetUserWishlistAsync(GetUserId());
            return Ok(ApiResponse.SuccessResponse(wishlist, "Wishlist retrieved successfully"));
        }

        [HttpPost("user/toggle/{productId}")]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var userId = GetUserId();
            var result = await _wishlistService.ToggleWishlistAsync(userId, productId);

            if (result == null)
                return Ok(ApiResponse.SuccessResponse(null, "Product removed from wishlist"));

            return Ok(ApiResponse.SuccessResponse(result, "Product added to wishlist"));
        }

        [HttpPost("user/move-to-cart/{productId}")]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            int userId = GetUserId();
            var cartItem = await _wishlistService.MoveToCartAsync(userId, productId);

            if (cartItem == null)
                return NotFound(ApiResponse.SuccessResponse(null, "Product not found in wishlist"));

            return Ok(ApiResponse.SuccessResponse(cartItem, "Product moved to cart"));
        }
    }
}
