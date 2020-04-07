using DummyPayIntegration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DummyPayIntegration.Domain
{
    public interface IDummyDomain
    {
        ILogger Logger { get; }

        IPaymentSettings paymentSettings { get; }

        IDummyModel dummyModel { get; }

        Task<PaymentCreationResultData> PaymentRequest(PaymentCreationRequestData requestData);

        Task<PaymentConfirmResultData> PaymentConfirm(PaymentConfirmRequestData requestData);

        Task<KeyValuePair<string, string>> AutoChallenge(Payment3DS payment3DS, string url);

        List<KeyValuePair<string, string>> GetKeyValuePairs(Payment3DS payment3DS);

        List<KeyValuePair<string, string>> ParseFormForRequest(string html);
    }
}
