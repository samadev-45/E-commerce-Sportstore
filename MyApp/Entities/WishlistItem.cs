using MyApp.Common;

namespace MyApp.Entities
{
    public class WishlistItem:BaseEntity
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int Quantity { get; set; } = 1;

    }
}
