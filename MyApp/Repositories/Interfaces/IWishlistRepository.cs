using MyApp.Entities;

namespace MyApp.Repositories.Interfaces
{
    public interface IWishlistRepository : IGenericRepository<WishlistItem>
    {
        Task<IEnumerable<WishlistItem>> GetWishlistByUserIdAsync(int userId);
        Task<WishlistItem?> GetWishlistItemAsync(int userId, int productId);
        Task RemoveAsync(WishlistItem wishlistItem);
    }
}
