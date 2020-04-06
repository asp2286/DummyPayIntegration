using DummyPayIntegration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DummyPayIntegration
{
    public interface IPaymentService
    {
        ILogger Logger { get; }
        IPaymentSettings paymentSettings { get; }
        Task<PaymentCreationResultData> CreatePayment(PaymentCreationRequestData requestData);
        Task<KeyValuePair<string, string>> AutoChallenge(Payment3DS payment3DS, string url);
        Task<PaymentConfirmResultData> ConfirmPayment(PaymentConfirmRequestData requestData);
    }
}
