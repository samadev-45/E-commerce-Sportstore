using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Wishlist;
using MyApp.DTOs.Cart;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly IGenericRepository<WishlistItem> _wishlistRepository;
        private readonly IGenericRepository<CartItem> _cartRepository;
        private readonly IGenericRepository<Product> _productRepository;

        public WishlistService(
            IGenericRepository<WishlistItem> wishlistRepository,
            IGenericRepository<CartItem> cartRepository,
            IGenericRepository<Product> productRepository
        )
        {
            _wishlistRepository = wishlistRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<WishlistItemDto?> ToggleWishlistAsync(int userId, int productId)
        {
            var existing = await _wishlistRepository.Query()
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existing != null)
            {
                await _wishlistRepository.DeleteAsync(existing);
                await _wishlistRepository.SaveChangesAsync();
                return null;
            }

            var wishlistItem = new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = 1
            };

            await _wishlistRepository.AddAsync(wishlistItem);
            await _wishlistRepository.SaveChangesAsync();

            var product = await _productRepository.Query()
                .FirstOrDefaultAsync(p => p.Id == productId);

            return new WishlistItemDto
            {
                Id = wishlistItem.Id,
                ProductId = productId,
                ProductName = product?.Name ?? "",
                Price = product?.Price ?? 0,
                Quantity = wishlistItem.Quantity
            };
        }

        public async Task<List<WishlistItemDto>> GetUserWishlistAsync(int userId)
        {
            var items = await _wishlistRepository.Query()
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                .ToListAsync();

            return items.Select(w => new WishlistItemDto
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product?.Name ?? "",
                Price = w.Product?.Price ?? 0,
                Quantity = w.Quantity
            }).ToList();
        }

        public async Task<CartItemDto?> MoveToCartAsync(int userId, int productId)
        {
            var wishlistItem = await _wishlistRepository.Query()
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem == null) return null;

            int quantityToMove = wishlistItem.Quantity;
            await _wishlistRepository.DeleteAsync(wishlistItem);

            var existingCartItem = await _cartRepository.Query()
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            CartItem cartItem;

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantityToMove;
                await _cartRepository.UpdateAsync(existingCartItem);
                cartItem = existingCartItem;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantityToMove
                };
                await _cartRepository.AddAsync(cartItem);
            }

            await _wishlistRepository.SaveChangesAsync();
            await _cartRepository.SaveChangesAsync();

            var product = wishlistItem.Product ?? existingCartItem?.Product;

            return new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = product?.Name ?? "",
                Price = product?.Price ?? 0,
                Quantity = cartItem.Quantity
            };
        }

        public async Task UpdateWishlistQuantityAsync(int userId, int productId, int quantity)
        {
            var item = await _wishlistRepository.Query()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                await _wishlistRepository.UpdateAsync(item);
                await _wishlistRepository.SaveChangesAsync();
            }
        }
    }
}
