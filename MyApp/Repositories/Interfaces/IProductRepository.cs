using MyApp.Entities;

namespace MyApp.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        // Add product-specific method signatures here if needed
        // Example: Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    }
}
