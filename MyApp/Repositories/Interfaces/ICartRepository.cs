using MyApp.Entities;

namespace MyApp.Repositories.Interfaces
{
    public interface ICartRepository : IGenericRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartByUserIdAsync(int userId);
        Task<CartItem?> GetCartItemAsync(int userId, int productId);
        Task RemoveAsync(CartItem cartItem);
    }
}
