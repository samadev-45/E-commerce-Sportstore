using MyApp.DTOs.Products;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;

namespace MyApp.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            return products.Select(ToDto);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            return product == null ? null : ToDto(product);
        }

        public async Task<ProductDto> AddAsync(ProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl
            };

            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();

            dto.Id = product.Id;
            return dto;
        }

        public async Task<ProductDto?> UpdateAsync(int id, ProductDto dto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Category = dto.Category;
            product.Price = dto.Price;
            product.ImageUrl = dto.ImageUrl;

            await _repository.UpdateAsync(product);
            await _repository.SaveChangesAsync();

            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return false;

            await _repository.DeleteAsync(product);
            await _repository.SaveChangesAsync();
            return true;
        }

        //  Search products by name and/or category
        public async Task<IEnumerable<ProductDto>> SearchAsync(string? name, string? category)
        {
            var products = await _repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(name))
                products = products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(category))
                products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            return products.Select(ToDto);
        }

        //  Get products by category only
        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
        {
            var products = await _repository.GetAllAsync();
            products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            return products.Select(ToDto);
        }

        //  Mapping helper
        private static ProductDto ToDto(Product p) => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Category = p.Category,
            Price = p.Price,
            ImageUrl = p.ImageUrl
        };
    }
}
