using MyApp.DTOs.Products;
using MyApp.Entities;

namespace MyApp.Services.Interfaces
{
    public interface IProductService : IGenericService<Product, ProductDto>
    {
        // Accept a filter DTO instead of just a string
        Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter);
    }
}
