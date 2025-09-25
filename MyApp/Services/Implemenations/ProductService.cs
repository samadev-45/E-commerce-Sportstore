using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Products;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services.Implementations
{
    public class ProductService : GenericService<Product, ProductDto>, IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            IImageService imageService,
            IHttpContextAccessor httpContextAccessor
        ) : base(productRepository, mapper, httpContextAccessor)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _imageService = imageService;
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

            
            product.CreatedOn = DateTime.UtcNow;
            product.CreatedBy = GetCurrentUserId();

            
            if (dto.Image != null)
            {
                try
                {
                    var result = await _imageService.UploadFromFileAsync(dto.Image, "products");
                    product.ImageUrl = result.SecureUrl?.ToString();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Image upload failed: {ex.Message}");
                }
            }

            // Save
            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || product.IsDeleted) return null;

            
            _mapper.Map(dto, product);

            
            product.ModifiedOn = DateTime.UtcNow;
            product.ModifiedBy = GetCurrentUserId();

            if (dto.Image != null)
            {
                try
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var publicId = ExtractPublicId(product.ImageUrl);
                        await _imageService.DeleteAsync(publicId);
                    }

                    var result = await _imageService.UploadFromFileAsync(dto.Image, "products", product.Id.ToString());
                    product.ImageUrl = result.SecureUrl?.ToString();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Image update failed: {ex.Message}");
                }
            }

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }


        public override async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || product.IsDeleted) return false;

            // Delete image from Cloudinary if exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var publicId = ExtractPublicId(product.ImageUrl);
                await _imageService.DeleteAsync(publicId);
            }

            product.IsDeleted = true;
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || !product.IsDeleted) return false;

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

        private string ExtractPublicId(string url)
        {
            
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var parts = path.Split('/');
            if (parts.Length >= 4) // 
            {
                var folder = parts[parts.Length - 2];
                var id = parts[parts.Length - 1].Split('.')[0]; 
                return $"{folder}/{id}";
            }
            return path.Split('/').Last().Split('.').First(); 
        }
    }
}