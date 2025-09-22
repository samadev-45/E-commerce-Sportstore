using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Products;
using MyApp.Services.Interfaces;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/Products
        [HttpGet]
        [AllowAnonymous] // Anyone can view products
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<ProductDto>>(products, true, 200, "Products retrieved successfully"));
        }

        // GET: api/Products/{id}
        [HttpGet("{id}")]
        [AllowAnonymous] // Anyone can view product details
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound(new ApiResponse<ProductDto>(null, false, 404, "Product not found"));

            return Ok(new ApiResponse<ProductDto>(product, true, 200, "Product retrieved successfully"));
        }

        // POST: api/Products
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can create
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<ProductDto>(null, false, 400, "Invalid product data"));

            var product = await _productService.CreateAsync(dto);
            return StatusCode(201, new ApiResponse<ProductDto>(product, true, 201, "Product created successfully"));
        }

        // PUT: api/Products/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can update
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<bool?>(null, false, 400, "Invalid product data"));

            var updated = await _productService.UpdateAsync(id, dto);
            if (updated != true)
                return NotFound(new ApiResponse<bool?>(false, false, 404, "Product not found or deleted"));

            return Ok(new ApiResponse<bool?>(true, true, 200, "Product updated successfully"));
        }

        // DELETE: api/Products/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<bool>(false, false, 404, "Product not found or already deleted"));

            return Ok(new ApiResponse<bool>(true, true, 200, "Product deleted successfully"));
        }

        // PUT: api/Products/restore/{id}
        [HttpPut("restore/{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can restore
        public async Task<IActionResult> Restore(int id)
        {
            var restored = await _productService.RestoreAsync(id);
            if (!restored)
                return NotFound(new ApiResponse<bool>(false, false, 404, "Product not found or not deleted"));

            return Ok(new ApiResponse<bool>(true, true, 200, "Product restored successfully"));
        }

        // GET: api/Products/search
        [HttpGet("search")]
        [AllowAnonymous] // Anyone can search
        public async Task<IActionResult> Search(
            [FromQuery] string? name,
            [FromQuery] string? category,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var filter = new ProductFilterDto
            {
                Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var results = await _productService.SearchAsync(filter);
            return Ok(new ApiResponse<IEnumerable<ProductDto>>(results, true, 200, "Products retrieved successfully"));
        }
    }
}
