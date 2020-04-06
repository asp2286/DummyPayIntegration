using System;
using System.Collections.Generic;
using System.Text;

namespace DummyPayIntegration.Models
{
    public class PaymentCreationResultData
    {
        public string TransactionId { get; set; }
        public string TransactionStatus { get; set; }
        public string PaReq { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
    }
    public class PaymentCreationResponseData : Error
    {
        public PaymentCreationResultData Result { get; set; }
    }
}
