using MyApp.DTOs.Wishlist;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp.Services.Interfaces
{
    public interface IWishlistService
    {
        /// <summary>
        /// Toggle a product in the user's wishlist.
        /// If the product exists, remove it; otherwise, add it.
        /// Returns the added WishlistItemDto if added, null if removed.
        /// </summary>
        Task<WishlistItemDto?> ToggleWishlistAsync(int userId, int productId);

        /// <summary>
        /// Get all wishlist items for a specific user.
        /// </summary>
        Task<List<WishlistItemDto>> GetUserWishlistAsync(int userId);

        /// <summary>
        /// Move a wishlist item to cart (removes from wishlist).
        /// </summary>
        Task MoveToCartAsync(int userId, int productId);
    }
}
