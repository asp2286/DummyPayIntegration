using System;
using System.Collections.Generic;
using System.Text;

namespace DummyPayIntegration
{
    public class PaymentSettings : IPaymentSettings
    {
        public string MerchantId { get; set; }
        public string SecretKey { get; set; }
        public Uri BaseAddress { get; set; }
    }
}
