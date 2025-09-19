using AutoMapper;
using MyApp.DTOs.Products;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;

namespace MyApp.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            return _mapper.Map<ProductDto?>(product);
        }

        public async Task<ProductDto> AddAsync(ProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, ProductDto dto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;

            _mapper.Map(dto, product); // updates entity fields
            await _repository.UpdateAsync(product);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return false;

            await _repository.DeleteAsync(product);
            await _repository.SaveChangesAsync();
            return true;
        }

        //  Search + filter
        public async Task<IEnumerable<ProductDto>> SearchAsync(ProductFilterDto filter)
        {
            var products = await _repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                products = products.Where(p => p.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(filter.Category))
                products = products.Where(p => p.Category.Equals(filter.Category, StringComparison.OrdinalIgnoreCase));

            if (filter.MinPrice.HasValue)
                products = products.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                products = products.Where(p => p.Price <= filter.MaxPrice.Value);

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
        {
            var products = await _repository.GetAllAsync();
            products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}
