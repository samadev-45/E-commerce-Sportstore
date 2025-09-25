using MyApp.Common;
using MyApp.Common.Enums;
using System;
using System.Collections.Generic;

namespace MyApp.Entities
{
    public class Order : BaseEntity
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Address { get; set; } = string.Empty;

        // Default status using OrderStatus helper
        public string Status { get; set; } = OrderStatus.Pending.ToString();

        public string PaymentType { get; set; } = "COD";
        public decimal TotalPrice { get; set; }
        public string? PaymentId { get; set; }
        

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
