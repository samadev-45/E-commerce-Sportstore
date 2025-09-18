using MyApp.Data;

using Microsoft.EntityFrameworkCore;

using MyApp.Entities;
using MyApp.Repositories.Interfaces;

namespace MyApp.Repositories.Implementations
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly AppDbContext _context;
        public WishlistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WishlistItem>> GetUserWishlistAsync(int userId)
        {
            return await _context.WishlistItems
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<WishlistItem?> GetWishlistItemAsync(int userId, int productId)
        {
            return await _context.WishlistItems
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task AddAsync(WishlistItem item)
        {
            await _context.WishlistItems.AddAsync(item);
        }

        public async Task RemoveAsync(WishlistItem item)
        {
            _context.WishlistItems.Remove(item);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
