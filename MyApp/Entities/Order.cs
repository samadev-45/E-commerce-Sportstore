using System;
using System.Collections.Generic;

namespace EcommerceAPI.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
