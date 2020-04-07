using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyPayIntegration.Models
{
    public interface IDummyModel
    {
        ILogger Logger { get; }
        IPaymentSettings paymentSettings { get; }
        Task<PaymentCreationResponseData> PaymentRequest(PaymentCreationRequestData requestData);
        Task<KeyValuePair<Cookie, string>> GetFormContent(List<KeyValuePair<string, string>> kvp, string url);
        Task<KeyValuePair<string, string>> Send3DSConfirm(Cookie cookie, List<KeyValuePair<string, string>> kvp, string lastUrl);
        Task<PaymentConfirmResponseData> PaymentConfirm(PaymentConfirmRequestData requestData);
    }
}
