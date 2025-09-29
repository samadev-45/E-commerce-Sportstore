namespace MyApp.DTOs.Products
{
    public class ProductImageDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string Base64Data { get; set; } = string.Empty; // Base64 string for image
        public IFormFile File { get; set; } = null!;
    }
}
