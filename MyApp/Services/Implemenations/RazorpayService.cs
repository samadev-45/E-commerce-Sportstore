using Microsoft.Extensions.Options;
using Razorpay.Api;
using MyApp.Helpers;
using MyApp.Services.Interfaces;
using System.Collections.Generic;

namespace MyApp.Services.Implementations
{
    public class RazorpayService : IRazorpayService
    {
        private readonly RazorpaySettings _settings;

        public RazorpayService(IOptions<RazorpaySettings> options)
        {
            _settings = options.Value;
        }

        private RazorpayClient GetClient() => new RazorpayClient(_settings.Key, _settings.Secret);

        public Order CreateOrder(int amountInPaise, string currency, string receipt = "")
        {
            var client = GetClient();

            var options = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", currency },
                { "receipt", string.IsNullOrEmpty(receipt) ? "receipt_" + System.DateTime.UtcNow.Ticks : receipt },
                { "payment_capture", 1 } // auto capture
            };

            return client.Order.Create(options);
        }
    }
}
