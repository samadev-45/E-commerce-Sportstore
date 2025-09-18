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
        private readonly IMapper _mapper;

        public WishlistService(IWishlistRepository wishlistRepo, IProductRepository productRepo, IMapper mapper)
        {
            _wishlistRepo = wishlistRepo;
            _productRepo = productRepo;
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
    }
}
