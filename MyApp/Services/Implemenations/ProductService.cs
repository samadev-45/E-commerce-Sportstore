using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyApp.DTOs.Products;
using MyApp.Entities;
using MyApp.Repositories.Interfaces;
using MyApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                .Include(p => p.ProductImages)
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            // Map images to Base64
            foreach (var dto in dtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                dto.ImagesBase64 = product.ProductImages
                                          .Select(pi => Convert.ToBase64String(pi.FileData))
                                          .ToList();
            }

            return dtos;
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            product.CreatedOn = DateTime.UtcNow;
            product.CreatedBy = GetCurrentUserId();

            // Add multiple images
            foreach (var file in dto.Images)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var productImage = new ProductImage
                {
                    Product = product,
                    FileName = Path.GetFileNameWithoutExtension(file.FileName),
                    FileExtension = Path.GetExtension(file.FileName),
                    FileData = ms.ToArray()
                };

                product.ProductImages.Add(productImage);
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            var resultDto = _mapper.Map<ProductDto>(product);
            resultDto.ImagesBase64 = product.ProductImages
                                            .Select(pi => Convert.ToBase64String(pi.FileData))
                                            .ToList();

            return resultDto;
        }

        public async Task<bool?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _productRepository.Query()
                                                  .Include(p => p.ProductImages)
                                                  .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.IsDeleted) return null;

            _mapper.Map(dto, product);
            product.ModifiedOn = DateTime.UtcNow;
            product.ModifiedBy = GetCurrentUserId();

            // Add new images if provided
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var file in dto.Images)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        FileName = Path.GetFileNameWithoutExtension(file.FileName),
                        FileExtension = Path.GetExtension(file.FileName),
                        FileData = ms.ToArray()
                    };

                    product.ProductImages.Add(productImage);
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

            product.IsDeleted = true;
            product.DeletedOn = DateTime.UtcNow;
            product.DeletedBy = GetCurrentUserId();

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
            var query = _productRepository.Query().Include(p => p.ProductImages).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(p => p.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.Category))
                query = query.Where(p => p.Category.Contains(filter.Category));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            query = query.Where(p => !p.IsDeleted);

            var products = await query.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            foreach (var dto in dtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                dto.ImagesBase64 = product.ProductImages
                                          .Select(pi => Convert.ToBase64String(pi.FileData))
                                          .ToList();
            }

            return dtos;
        }
        public async Task<bool> AddImagesAsync(int productId, List<IFormFile> images)
        {
            var product = await _productRepository.Query()
                                                  .Include(p => p.ProductImages)
                                                  .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return false;

            foreach (var file in images)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var productImage = new ProductImage
                {
                    ProductId = product.Id,
                    FileName = Path.GetFileNameWithoutExtension(file.FileName),
                    FileExtension = Path.GetExtension(file.FileName),
                    FileData = ms.ToArray()
                };

                product.ProductImages.Add(productImage);
            }

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteImageAsync(int productId, int imageId)
        {
            var product = await _productRepository.Query()
                                                  .Include(p => p.ProductImages)
                                                  .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return false;

            var image = product.ProductImages.FirstOrDefault(pi => pi.Id == imageId);
            if (image == null) return false;

            product.ProductImages.Remove(image);

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }


    }
}
