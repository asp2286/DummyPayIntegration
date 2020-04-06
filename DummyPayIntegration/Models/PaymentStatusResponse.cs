using System;
using System.Collections.Generic;
using System.Text;

namespace DummyPayIntegration.Models
{
    public class PaymentStatusResponse
    {
        public string TransactionId;
        public string Status;
        public decimal Amount;
        public string Currency;
        public string OrderId;
        public string LastFourDigits;
    }
}
