using MyApp.Entities;

namespace MyApp.Repositories.Interfaces
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<WishlistItem>> GetUserWishlistAsync(int userId);
        Task<WishlistItem?> GetWishlistItemAsync(int userId, int productId);
        Task AddAsync(WishlistItem item);
        Task RemoveAsync(WishlistItem item);
        Task SaveChangesAsync();
    }
}
