using MyApp.Common.Enums;

namespace MyApp.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = OrderStatus.Pending.ToString();
        public decimal TotalPrice { get; set; }
        public string PaymentType { get; set; } = "COD";
        public string? PaymentId { get; set; }
        public string? FundAccountId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
