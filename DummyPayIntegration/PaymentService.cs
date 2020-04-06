using DummyPayIntegration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DummyPayIntegration
{
    public class PaymentService : IPaymentService
    {
        public ILogger Logger { get; }
        public ILoggerFactory loggerFactory { get; }
        public IPaymentSettings paymentSettings { get; }
        private RequestingPayment requestingPayment { get; }
        public PaymentService(IPaymentSettings paymentSettings, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            Logger = loggerFactory?.CreateLogger<RequestingPayment>();
            if (Logger == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            this.paymentSettings = paymentSettings;
            requestingPayment = new RequestingPayment(loggerFactory);
        }

        public async Task<PaymentCreationResultData> CreatePayment(PaymentCreationRequestData requestData)
        {
            try
            {
                Logger.LogInformation("Trying to create reguest");
                return (await requestingPayment.PaymentRequest(requestData, paymentSettings)).Result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<KeyValuePair<string, string>> AutoChallenge(Payment3DS payment3DS, string url)
        {
            var processing3DS = new Processing3DS(loggerFactory, paymentSettings);
            var kvp = processing3DS.GetKeyValuePairs(payment3DS);
            var content = await requestingPayment.GetFormContent(paymentSettings, kvp, url);
            var nvc = processing3DS.ParseFormForRequest(content.Value);
            var res = await requestingPayment.Send3DSConfirm(content.Key, nvc, url);

            return res;
        }

        public async Task<PaymentConfirmResultData> ConfirmPayment(PaymentConfirmRequestData requestData)
        {
            try
            {
                Logger.LogInformation("Trying to create confirm reguest");
                return (await requestingPayment.PaymentConfirm(requestData, paymentSettings)).Result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
