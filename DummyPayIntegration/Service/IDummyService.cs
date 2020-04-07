using DummyPayIntegration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DummyPayIntegration.Service
{
    public interface IDummyService
    {
        ILogger Logger { get; }
        IPaymentSettings paymentSettings { get; }
        Task<PaymentConfirmResultData> CreatePayment(PaymentCreationRequestData requestData, string Md, string TermUrl);
    }
}
