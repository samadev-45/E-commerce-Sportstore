using MyApp.DTOs.Cart;
using MyApp.Entities;
using MyApp.Services.Interfaces;

public interface ICartService : IGenericService<CartItem, CartItemDto>
{
    Task<IEnumerable<CartItemDto>> GetUserCartAsync(int userId);
    Task AddToCartAsync(int userId, int productId, int quantity);
    Task RemoveFromCartAsync(int userId, int productId);
    Task UpdateQuantityAsync(int userId, int productId, int quantity);
}
