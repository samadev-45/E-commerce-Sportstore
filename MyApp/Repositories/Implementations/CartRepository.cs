using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;

namespace MyApp.Repositories.Implementations
{
    public class CartRepository : GenericRepository<CartItem>, ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetCartByUserIdAsync(int userId)
        {
            return await _context.CartItems
                .Include(c => c.Product) // load product details
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(int userId, int productId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task RemoveAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }
    }
}
