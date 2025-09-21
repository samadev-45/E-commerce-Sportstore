using Microsoft.EntityFrameworkCore;
using MyApp.Common;

namespace MyApp.Entities
{
    public class OrderItem :BaseEntity
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; } // price at purchase time
        public decimal UnitPrice { get; set; }
    }
}
