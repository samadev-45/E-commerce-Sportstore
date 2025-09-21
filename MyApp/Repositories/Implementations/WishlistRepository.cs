using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;

namespace MyApp.Repositories.Implementations
{
    public class WishlistRepository : GenericRepository<WishlistItem>, IWishlistRepository
    {
        private readonly AppDbContext _context;

        public WishlistRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WishlistItem>> GetWishlistByUserIdAsync(int userId)
        {
            return await _context.WishlistItems
                .Include(w => w.Product) // load product details
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<WishlistItem?> GetWishlistItemAsync(int userId, int productId)
        {
            return await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task RemoveAsync(WishlistItem wishlistItem)
        {
            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();
        }
    }
}
