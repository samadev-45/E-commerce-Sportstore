using Microsoft.EntityFrameworkCore;
using MyApp.Common;

namespace MyApp.Entities
{
    public class Product : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal Price { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // ✅ New navigation property for multiple images
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
