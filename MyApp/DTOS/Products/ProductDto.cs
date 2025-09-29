namespace MyApp.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Return images as Base64 string for display
        public List<string> ImagesBase64 { get; set; } = new();
    }
}
