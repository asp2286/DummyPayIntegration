using System;
using System.Collections.Generic;
using System.Text;

namespace DummyPayIntegration.Models
{
    public class PaymentConfirmResultData
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string OrderId { get; set; }
        public string LastFourDigits { get; set; }
    }
    public class PaymentConfirmResponseData : Error
    {
        public PaymentConfirmResultData Result { get; set; }
    }
}
