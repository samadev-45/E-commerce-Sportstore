using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
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

        public override async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.Query()
                                                   .Where(p => !p.IsDeleted)
                                                   .ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || product.IsDeleted)
                return null;

            _mapper.Map(dto, product);
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            return true;
        }

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

        public async Task<bool> RestoreAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || !product.IsDeleted)
                return false;

            product.IsDeleted = false;
            product.DeletedOn = null;
            product.DeletedBy = null;

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            return true;
        }

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
    }
}
