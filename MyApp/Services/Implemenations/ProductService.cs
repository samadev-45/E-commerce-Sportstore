using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Products;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;

namespace MyApp.Services.Implementations
{
    public class ProductService : GenericService<Product, ProductDto>, IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
        ) : base(productRepository, mapper, httpContextAccessor)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        // Override GetAllAsync -> exclude deleted products
        public override async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.Query()
                                                   .Where(p => !p.IsDeleted)
                                                   .ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        //  Override DeleteAsync -> soft delete
        public override async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || product.IsDeleted)
                return false;

            product.IsDeleted = true;
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            return true;
        }

        //  Search with filters and excluding deleted products
        public async Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter)
        {
            var query = _productRepository.Query().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(p => p.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.Category))
                query = query.Where(p => p.Category.Contains(filter.Category));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            query = query.Where(p => !p.IsDeleted);

            return _mapper.Map<IEnumerable<ProductDto>>(await query.ToListAsync());
        }

        //  (Optional) Restore product
        public async Task<bool> RestoreAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || !product.IsDeleted)
                return false;

            product.IsDeleted = false;
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            return true;
        }
    }
}
