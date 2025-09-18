namespace MyApp.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
