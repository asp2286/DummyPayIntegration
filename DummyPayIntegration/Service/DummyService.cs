using DummyPayIntegration.Models;
using DummyPayIntegration.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DummyPayIntegration.Service
{
    public class DummyService : IDummyService
    {
        public ILogger Logger { get; }
        public ILoggerFactory loggerFactory { get; }
        public IPaymentSettings paymentSettings { get; }
        private IDummyDomain dummyDomain { get; }
        public DummyService(IPaymentSettings paymentSettings, IDummyDomain dummyDomain, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            Logger = loggerFactory?.CreateLogger<DummyModel>();
            if (Logger == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            this.paymentSettings = paymentSettings;
            this.dummyDomain = dummyDomain;
        }

        public async Task<PaymentConfirmResultData> CreatePayment(PaymentCreationRequestData requestData, string Md, string TermUrl)
        {
            try
            {
                Logger.LogInformation("Trying to create reguest");
                var createResult = await dummyDomain.PaymentRequest(requestData);
                var payment3DS = new Payment3DS()
                {
                    MD = Md,
                    PaReq = createResult.PaReq,
                    TermUrl = TermUrl
                };
                var result3Ds = await dummyDomain.AutoChallenge(payment3DS, createResult.Url);
                var dataconfirm = new PaymentConfirmRequestData
                {
                    PaRes = result3Ds.Value,
                    TransactionId = createResult.TransactionId
                };
                Logger.LogInformation("Trying to create confirm reguest");
                var result = await dummyDomain.PaymentConfirm(dataconfirm);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
