using MyApp.Entities;

namespace MyApp.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartItem>> GetUserCartAsync(int userId);
        Task<CartItem?> GetCartItemAsync(int userId, int productId);
        Task AddAsync(CartItem item);
        Task RemoveAsync(CartItem item);
        Task ClearCartAsync(int userId);  
        Task SaveChangesAsync();
    }
}
