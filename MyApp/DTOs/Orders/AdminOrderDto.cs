namespace MyApp.DTOs.Orders
{
    // For admin order list view
    public class AdminOrderDto
    {
        public string OrderId { get; set; } = string.Empty; 
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<AdminOrderItemDto> Items { get; set; } = new();
        public string Address { get; set; } = string.Empty;
        public DateTime Time { get; set; } 
        public string Status { get; set; } = "Pending";
        


    }
}
