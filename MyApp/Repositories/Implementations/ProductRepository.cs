using MyApp.Data;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;

namespace MyApp.Repositories.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // Add product-specific methods here if needed
        // Example:
        // public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        // {
        //     return await _context.Products
        //         .Where(p => p.Category == category)
        //         .ToListAsync();
        // }
    }
}
