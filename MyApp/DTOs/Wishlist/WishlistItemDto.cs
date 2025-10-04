namespace MyApp.DTOs.Wishlist
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public List<string> ImagesBase64 { get; set; } = new();
        public string Category { get; set; } = string.Empty;


    }

    public class AddToWishlistDto
    {
        public int ProductId { get; set; }
    }
}
