using MyApp.DTOs.Cart;

namespace MyApp.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemDto>> GetUserCartAsync(int userId);
        Task<CartItemDto> AddToCartAsync(int userId, AddToCartDto dto);
        Task<bool> RemoveFromCartAsync(int userId, int productId);
    }
}
