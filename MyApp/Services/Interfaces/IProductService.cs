using MyApp.DTOs.Products;

namespace MyApp.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> AddAsync(ProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, ProductDto dto);
        Task<bool> DeleteAsync(int id);

		//Add products by category
		Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category);


		// search method
		Task<IEnumerable<ProductDto>> SearchAsync(string? name, string? category);
    }
}
