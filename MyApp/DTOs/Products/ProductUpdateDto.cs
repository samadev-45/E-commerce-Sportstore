namespace MyApp.DTOs.Products
{
    public class ProductUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Multiple images as IFormFile (replace/add)
        public List<IFormFile> Images { get; set; } = new();
    }
}
