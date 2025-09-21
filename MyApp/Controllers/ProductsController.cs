using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.DTOs.Products;
using MyApp.Helpers;
using MyApp.Services.Interfaces;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        // USER SECTION
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? name,
            [FromQuery] string? category,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var filter = new ProductFilterDto
            {
                Name = name,
                Category = category,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var products = (name != null || category != null || minPrice.HasValue || maxPrice.HasValue)
                ? await _service.SearchAsync(filter)
                : await _service.GetAllAsync();

            return this.OkResponse(products, "Products retrieved successfully");
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null)
                return this.BadResponse("Product not found", 404);

            return this.OkResponse(product, "Product retrieved successfully");
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> Filter([FromQuery] ProductFilterDto filter)
        {
            var products = await _service.SearchAsync(filter);
            return this.OkResponse(products, "Filtered products retrieved successfully");
        }

        // ADMIN SECTION
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return this.BadResponse("Invalid product data");

            var product = await _service.AddAsync(dto);
            return this.OkResponse(product, "Product created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return this.BadResponse("Invalid product data");

            var product = await _service.UpdateAsync(id, dto);
            if (product == null)
                return this.BadResponse("Product not found", 404);

            return this.OkResponse(product, "Product updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return this.BadResponse("Product not found", 404);

            return this.OkResponse<object>(null, "Product deleted successfully");
        }
    }
}
