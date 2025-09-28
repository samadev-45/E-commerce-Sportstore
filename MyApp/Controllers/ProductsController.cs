using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Products;
using MyApp.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        // -----------------------------
        // Get all products (public)
        // -----------------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse.SuccessResponse(products, "Products retrieved successfully"));
        }

        // -----------------------------
        // Get product by ID (public)
        // -----------------------------
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound(ApiResponse.FailResponse("Product not found", 404));

            return Ok(ApiResponse.SuccessResponse(product, "Product retrieved successfully"));
        }

        // -----------------------------
        // Create new product (Admin only)
        // -----------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.FailResponse("Invalid product data"));

            var product = await _productService.CreateAsync(dto);
            return StatusCode(201, ApiResponse.SuccessResponse(product, "Product created successfully"));
        }

        // -----------------------------
        // Update product (Admin only)
        // -----------------------------
        // Update product (Admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.FailResponse("Invalid product data"));

            var updated = (await _productService.UpdateAsync(id, dto)).GetValueOrDefault();// no GetValueOrDefault()
            if (!updated)
                return NotFound(ApiResponse.FailResponse("Product not found or deleted"));

            return Ok(ApiResponse.SuccessResponse(true, "Product updated successfully"));
        }

        // Delete product (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteAsync(id); // no GetValueOrDefault()
            if (!deleted)
                return NotFound(ApiResponse.FailResponse("Product not found or already deleted"));

            return Ok(ApiResponse.SuccessResponse(true, "Product deleted successfully"));
        }

        // Restore deleted product (Admin only)
        [HttpPut("restore/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            var restored = await _productService.RestoreAsync(id); // no GetValueOrDefault()
            if (!restored)
                return NotFound(ApiResponse.FailResponse("Product not found or not deleted"));

            return Ok(ApiResponse.SuccessResponse(true, "Product restored successfully"));
        }

        // -----------------------------
        // Search products (public)
        // -----------------------------
        [HttpGet("search")]
        [AllowAnonymous]
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
            return Ok(ApiResponse.SuccessResponse(results, "Products retrieved successfully"));
        }
    }
}
