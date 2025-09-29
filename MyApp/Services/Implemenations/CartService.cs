using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Cart;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services.Implementations
{
    public class CartService : GenericService<CartItem, CartItemDto>, ICartService
    {
        private readonly IGenericRepository<CartItem> _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(
            IGenericRepository<CartItem> cartRepository,
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
            var items = await _cartRepository.Query()
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CartItemDto>>(items);
        }

        // Updated: Returns CartItemDto after adding/updating
        public async Task<CartItemDto> AddToCartAsync(int userId, int productId, int quantity)
        {
            var existingItem = await _cartRepository.Query()
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            CartItem cartItem;

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                await _cartRepository.UpdateAsync(existingItem);
                cartItem = existingItem;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                await _cartRepository.AddAsync(cartItem);
            }

            await _cartRepository.SaveChangesAsync();

            // Fetch product details if not already loaded
            var product = cartItem.Product ?? await _productRepository.Query()
                .FirstOrDefaultAsync(p => p.Id == productId);

            return new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = product?.Name ?? "",
                Price = product?.Price ?? 0,
                Quantity = cartItem.Quantity
            };
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var item = await _cartRepository.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (item != null)
            {
                await _cartRepository.DeleteAsync(item);
                await _cartRepository.SaveChangesAsync();
            }
        }

        // Updated: Returns CartItemDto after updating quantity
        public async Task<CartItemDto?> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var item = await _cartRepository.Query()
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (item == null) return null;

            item.Quantity = quantity;
            await _cartRepository.SaveChangesAsync();

            var product = item.Product ?? await _productRepository.Query()
                .FirstOrDefaultAsync(p => p.Id == productId);

            return new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "",
                Price = product?.Price ?? 0,
                Quantity = item.Quantity
            };
        }
    }
}
