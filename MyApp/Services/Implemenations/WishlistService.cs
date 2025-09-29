using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.DTOs.Wishlist;
using MyApp.Entities;
using MyApp.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly AppDbContext _context;

        public WishlistService(AppDbContext context)
        {
            _context = context;
        }

        // Toggle wishlist: Add if not exists, remove if exists
        public async Task<WishlistItemDto?> ToggleWishlistAsync(int userId, int productId)
        {
            var existing = await _context.WishlistItems
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existing != null)
            {
                _context.WishlistItems.Remove(existing);
                await _context.SaveChangesAsync();
                return null; // removed
            }
            else
            {
                var wishlistItem = new WishlistItem
                {
                    UserId = userId,
                    ProductId = productId
                };
                await _context.WishlistItems.AddAsync(wishlistItem);
                await _context.SaveChangesAsync();

                var product = await _context.Products.FindAsync(productId);

                return new WishlistItemDto
                {
                    Id = wishlistItem.Id,
                    ProductId = productId,
                    ProductName = product?.Name ?? "",
                    Price = product?.Price ?? 0
                };
            }
        }

        public async Task<List<WishlistItemDto>> GetUserWishlistAsync(int userId)
        {
            return await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                .Select(w => new WishlistItemDto
                {
                    Id = w.Id,
                    ProductId = w.ProductId,
                    ProductName = w.Product.Name,
                    Price = w.Product.Price
                })
                .ToListAsync();
        }

        public async Task MoveToCartAsync(int userId, int productId)
        {
            var wishlistItem = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem != null)
            {
                _context.WishlistItems.Remove(wishlistItem);
                // Here, you can also add logic to add the product to the user's cart
                await _context.SaveChangesAsync();
            }
        }
    }
}
