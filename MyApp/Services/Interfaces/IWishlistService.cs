
using MyApp.DTOs.Wishlist;

namespace MyApp.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(int userId);
        Task<WishlistItemDto> AddToWishlistAsync(int userId, AddToWishlistDto dto);
        Task<bool> RemoveFromWishlistAsync(int userId, int productId);
        Task<bool> MoveToCartAsync(int userId, int productId);
    }
}
