using Microsoft.AspNetCore.Http;
using MyApp.DTOs.Products;
using MyApp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp.Services.Interfaces
{
    public interface IProductService : IGenericService<Product, ProductDto>
    {
        Task<ProductDto> CreateAsync(ProductCreateDto dto);
        Task<bool?> UpdateAsync(int id, ProductUpdateDto dto);
        Task<bool> RestoreAsync(int id);
        Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter);

        // New methods for multiple images
        Task<bool> AddImagesAsync(int productId, List<IFormFile> images);
        Task<bool> DeleteImageAsync(int productId, int imageId);
    }
}
