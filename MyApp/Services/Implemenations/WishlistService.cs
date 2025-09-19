using AutoMapper;
using MyApp.DTOs.Wishlist;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;

namespace MyApp.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly IProductRepository _productRepo;
        private readonly ICartRepository _cartRepo;  // 
        private readonly IMapper _mapper;

        public WishlistService(
            IWishlistRepository wishlistRepo,
            IProductRepository productRepo,
            ICartRepository cartRepo,
            IMapper mapper)
        {
            _wishlistRepo = wishlistRepo;
            _productRepo = productRepo;
            _cartRepo = cartRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(int userId)
        {
            var items = await _wishlistRepo.GetUserWishlistAsync(userId);
            return _mapper.Map<IEnumerable<WishlistItemDto>>(items);
        }

        public async Task<WishlistItemDto> AddToWishlistAsync(int userId, AddToWishlistDto dto)
        {
            var existingItem = await _wishlistRepo.GetWishlistItemAsync(userId, dto.ProductId);
            if (existingItem != null)
                return _mapper.Map<WishlistItemDto>(existingItem);

            var product = await _productRepo.GetByIdAsync(dto.ProductId)
                          ?? throw new Exception("Product not found");

            var wishlistItem = _mapper.Map<WishlistItem>(dto);
            wishlistItem.UserId = userId;

            await _wishlistRepo.AddAsync(wishlistItem);
            await _wishlistRepo.SaveChangesAsync();

            return _mapper.Map<WishlistItemDto>(wishlistItem);
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
        {
            var item = await _wishlistRepo.GetWishlistItemAsync(userId, productId);
            if (item == null) return false;

            await _wishlistRepo.RemoveAsync(item);
            await _wishlistRepo.SaveChangesAsync();
            return true;
        }

        // Move wishlist item to cart
        public async Task<bool> MoveToCartAsync(int userId, int productId)
        {
            var wishlistItem = await _wishlistRepo.GetWishlistItemAsync(userId, productId);
            if (wishlistItem == null) return false;

            // check if already in cart
            var cartItem = await _cartRepo.GetCartItemAsync(userId, productId);
            if (cartItem == null)
            {
                await _cartRepo.AddAsync(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity += 1;
            }

            await _cartRepo.SaveChangesAsync();

            // remove from wishlist
            await _wishlistRepo.RemoveAsync(wishlistItem);
            await _wishlistRepo.SaveChangesAsync();

            return true;
        }
    }
}
