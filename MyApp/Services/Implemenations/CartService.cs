using AutoMapper;
using MyApp.DTOs.Cart;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;

namespace MyApp.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;

        public CartService(ICartRepository cartRepo, IProductRepository productRepo, IMapper mapper)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItemDto>> GetUserCartAsync(int userId)
        {
            var cartItems = await _cartRepo.GetUserCartAsync(userId);
            return _mapper.Map<IEnumerable<CartItemDto>>(cartItems);
        }

        public async Task<CartItemDto> AddToCartAsync(int userId, AddToCartDto dto)
        {
            var existingItem = await _cartRepo.GetCartItemAsync(userId, dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                var product = await _productRepo.GetByIdAsync(dto.ProductId)
                              ?? throw new Exception("Product not found");

                existingItem = _mapper.Map<CartItem>(dto);
                existingItem.UserId = userId;

                await _cartRepo.AddAsync(existingItem);
            }

            await _cartRepo.SaveChangesAsync();
            return _mapper.Map<CartItemDto>(existingItem);
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int productId)
        {
            var item = await _cartRepo.GetCartItemAsync(userId, productId);
            if (item == null) return false;

            await _cartRepo.RemoveAsync(item);
            await _cartRepo.SaveChangesAsync();
            return true;
        }
        public async Task<CartItemDto?> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var item = await _cartRepo.GetCartItemAsync(userId, productId);
            if (item == null) return null;

            item.Quantity = quantity;
            await _cartRepo.SaveChangesAsync();

            return _mapper.Map<CartItemDto>(item);
        }

    }
}
