using EcommerceAPI.DTOs.Products;

namespace EcommerceAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> AddAsync(ProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, ProductDto dto);
        Task<bool> DeleteAsync(int id);

        // 🔍 New
        Task<IEnumerable<ProductDto>> SearchAsync(string? name, string? category);
    }
}
