using MyApp.DTOs.Wishlist;
using MyApp.DTOs.Cart;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<WishlistItemDto?> ToggleWishlistAsync(int userId, int productId);
        Task<List<WishlistItemDto>> GetUserWishlistAsync(int userId);
        Task<CartItemDto?> MoveToCartAsync(int userId, int productId); // updated return type
        Task UpdateWishlistQuantityAsync(int userId, int productId, int quantity);
    }
}
