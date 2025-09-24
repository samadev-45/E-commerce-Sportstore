using Razorpay.Api;

namespace MyApp.Services.Interfaces
{
    public interface IRazorpayService
    {
        /// <summary>
        /// Create a test order in Razorpay
        /// </summary>
        /// <param name="amountInPaise"></param>
        /// <param name="currency"></param>
        /// <param name="receipt"></param>
        /// <returns>Razorpay.Api.Order</returns>
        Order CreateOrder(int amountInPaise, string currency, string receipt = "");
    }
}
