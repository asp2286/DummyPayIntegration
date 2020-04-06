using System;
using System.Collections.Generic;
using System.Text;

namespace DummyPayIntegration.Models
{
    public class PaymentConfirmRequestData
    {
        public string TransactionId { get; set; }
        public string PaRes { get; set; }
    }
}
