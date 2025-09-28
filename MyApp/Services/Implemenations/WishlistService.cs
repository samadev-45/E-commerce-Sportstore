using AutoMapper;
using MyApp.DTOs.Wishlist;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MyApp.Services.Implementations
{
    public class WishlistService : GenericService<WishlistItem, WishlistItemDto>, IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ICartRepository _cartRepository;

        public WishlistService(
            IWishlistRepository wishlistRepository,
            ICartRepository cartRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
        ) : base(wishlistRepository, mapper, httpContextAccessor)
        {
            _wishlistRepository = wishlistRepository;
            _cartRepository = cartRepository;
        }

        public async Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(int userId)
        {
            var items = await _wishlistRepository.GetWishlistByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<WishlistItemDto>>(items);
        }

        public async Task AddToWishlistAsync(int userId, int productId)
        {
            var existingItem = await _wishlistRepository.GetWishlistItemAsync(userId, productId);
            if (existingItem == null)
            {
                var newItem = new WishlistItem
                {
                    UserId = userId,
                    ProductId = productId
                };
                await _wishlistRepository.AddAsync(newItem);
                await _wishlistRepository.SaveChangesAsync();
            }
        }

        public async Task RemoveFromWishlistAsync(int userId, int productId)
        {
            var item = await _wishlistRepository.GetWishlistItemAsync(userId, productId);
            if (item != null)
            {
                await _wishlistRepository.DeleteAsync(item);
                await _wishlistRepository.SaveChangesAsync();
            }
        }

        public async Task MoveToCartAsync(int userId, int productId)
        {
            // 1️⃣ Check if the product is already in the cart
            var existingCartItem = await _cartRepository.GetCartItemAsync(userId, productId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1; // EF tracks this change automatically
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1
                };
                await _cartRepository.AddAsync(cartItem);
            }

            // 2️⃣ Remove from wishlist
            var wishlistItem = await _wishlistRepository.GetWishlistItemAsync(userId, productId);
            if (wishlistItem != null)
            {
                await _wishlistRepository.DeleteAsync(wishlistItem);
            }

            // 3️⃣ Save all changes
            await _cartRepository.SaveChangesAsync();
            await _wishlistRepository.SaveChangesAsync();
        }


    }
}
