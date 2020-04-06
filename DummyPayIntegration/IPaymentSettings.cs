using System;
using System.Collections.Generic;
using System.Text;

namespace DummyPayIntegration
{
    public interface IPaymentSettings
    {
        string MerchantId { get; set; }
        string SecretKey { get; set; }
        Uri BaseAddress { get; set; }
    }
}
