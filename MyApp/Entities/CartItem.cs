using System.ComponentModel.DataAnnotations.Schema;
using MyApp.Common;
namespace MyApp.Entities
{
    public class CartItem :BaseEntity
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Column(TypeName = "int")]
        public int Quantity { get; set; }
    }
}
