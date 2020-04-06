using System;
using System.Collections.Generic;
using System.Text;

namespace DummyPayIntegration
{
    public class PaymentException : Exception
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
    }
}
