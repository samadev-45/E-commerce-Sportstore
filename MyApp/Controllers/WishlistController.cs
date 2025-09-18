
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs.Wishlist;
using MyApp.Services.Interfaces;
using System.Security.Claims;

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

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserWishlist()
        {
            var wishlist = await _wishlistService.GetUserWishlistAsync(GetUserId());
            return Ok(wishlist);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(AddToWishlistDto dto)
        {
            var item = await _wishlistService.AddToWishlistAsync(GetUserId(), dto);
            return Ok(item);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var success = await _wishlistService.RemoveFromWishlistAsync(GetUserId(), productId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
