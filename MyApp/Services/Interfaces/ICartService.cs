using MyApp.DTOs.Cart;
using MyApp.Entities;
using MyApp.Services.Interfaces;

public interface ICartService : IGenericService<CartItem, CartItemDto>
{
    Task<IEnumerable<CartItemDto>> GetUserCartAsync(int userId);
    Task<CartItemDto> AddToCartAsync(int userId, int productId, int quantity); // updated
    Task RemoveFromCartAsync(int userId, int productId);
    Task<CartItemDto?> UpdateQuantityAsync(int userId, int productId, int quantity); // updated
}
