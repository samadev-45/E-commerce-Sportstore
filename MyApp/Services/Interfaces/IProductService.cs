using MyApp.DTOs.Products;
using MyApp.Entities;

namespace MyApp.Services.Interfaces
{
    public interface IProductService : IGenericService<Product, ProductDto>
    {
        // Public side
        Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter);

        // Admin side
        Task<ProductDto> CreateAsync(ProductCreateDto dto);
        Task<bool?> UpdateAsync(int id, ProductUpdateDto? productDto);

        Task<bool> RestoreAsync(int id);
        
    }
}
