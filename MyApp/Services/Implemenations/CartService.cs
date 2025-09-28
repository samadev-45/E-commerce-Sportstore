using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Cart;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MyApp.Services.Implementations
{
    public class CartService : GenericService<CartItem, CartItemDto>, ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
        ) : base(cartRepository, mapper, httpContextAccessor)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<CartItemDto>> GetUserCartAsync(int userId)
        {
            var items = await _cartRepository.GetCartByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<CartItemDto>>(items);
        }

        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            var existingItem = await _cartRepository.GetCartItemAsync(userId, productId);

            if (existingItem != null)
            {
                //  If the product is already in cart, just increase the quantity
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                await _cartRepository.AddAsync(newItem);
            }

            // EF Core tracks changes automatically, just save
            await _cartRepository.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var item = await _cartRepository.GetCartItemAsync(userId, productId);
            if (item != null)
            {
                await _cartRepository.DeleteAsync(item);
                await _cartRepository.SaveChangesAsync();
            }
        }

        public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var item = await _cartRepository.GetCartItemAsync(userId, productId);
            if (item != null)
            {
                item.Quantity = quantity;
                // No UpdateAsync needed; EF Core tracks changes
                await _cartRepository.SaveChangesAsync();
            }
        }
    }
}
