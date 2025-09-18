using MyApp.DTOs.Products;
using MyApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        // -------------------------------

       
        /// Get all products or search by name/category. (User)
      
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] string? name, [FromQuery] string? category)
        {
            if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(category))
            {
                var results = await _service.SearchAsync(name, category);
                return Ok(results);
            }

            var products = await _service.GetAllAsync();
            return Ok(products);
        }

        
        /// Get product by Id. (User)
       
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

       
        /// Get products by category. (User)
      
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var products = await _service.GetByCategoryAsync(category);
            return Ok(products);
        }

       
        // ADMIN SECTION
        // -------------------------------

        
        /// Create a new product. (Admin only)
        
        [HttpPost]
        // [Authorize(Roles = "Admin")]  
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        
        /// Update an existing product. (Admin only)
        
        [HttpPut("{id}")]
        // [Authorize(Roles = "Admin")]  
        public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = await _service.UpdateAsync(id, dto);
            if (product == null) return NotFound();

            return Ok(product);
        }

        
        /// Delete a product. (Admin only)
        
        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin")]  
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
