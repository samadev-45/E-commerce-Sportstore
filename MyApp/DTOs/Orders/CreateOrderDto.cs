namespace MyApp.DTOs.Orders
{
    public class CreateOrderDto
    {
        public string Address { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "COD"; // "COD" or "Online"
        
        
    }
}
