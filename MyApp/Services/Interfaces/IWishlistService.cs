using MyApp.DTOs.Wishlist;
using MyApp.Entities;

namespace MyApp.Services.Interfaces
{
    public interface IWishlistService : IGenericService<WishlistItem, WishlistItemDto>
    {
        Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(int userId);
        Task AddToWishlistAsync(int userId, int productId);
        Task RemoveFromWishlistAsync(int userId, int productId);
        Task MoveToCartAsync(int userId, int productId);
    }
}
