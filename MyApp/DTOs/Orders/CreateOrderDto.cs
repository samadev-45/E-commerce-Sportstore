using MyApp.Common.Enums;

namespace MyApp.DTOs.Orders
{
    public class CreateOrderDto
    {
        public string Address { get; set; } = string.Empty;
        
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;


    }
}
