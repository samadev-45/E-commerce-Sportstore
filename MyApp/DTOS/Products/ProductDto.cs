namespace MyApp.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }   // read/update
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
