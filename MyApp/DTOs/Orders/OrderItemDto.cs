namespace MyApp.DTOs.Orders
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // mapped from Product.Name
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
