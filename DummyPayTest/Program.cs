using System;
using System.Threading.Tasks;
using DummyPayIntegration;
using DummyPayIntegration.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DummyPayTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Application application = new Application(serviceCollection);
        }

        static private void ConfigureServices(IServiceCollection serviceCollection)
        {
            Log.Logger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            ILoggerFactory loggerFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory(Log.Logger);
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
        }
    }

    public class Application
    {
        public IServiceProvider Services { get; set; }
        public ILogger Logger { get; set; }
        public Application(IServiceCollection serviceCollection)
        {
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();
            Logger = Services.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<Application>();
            Logger.LogInformation("Application created successfully.");
            var paymentService = Services.GetRequiredService<IPaymentService>();
            var data = new PaymentCreationRequestData()
            {
                OrderId = "DBB99946-A10A-4D1B-A742-577FA026BC01",
                Amount = 12312,
                Currency = "USD",
                Country = "CY",
                CardNumber = "4111111111111111",
                CardExpiryDate = "1123",
                CardHolder = "TEST TESTER",
                Cvv = "111"
            };
            try
            {
                var result = paymentService.CreatePayment(data).GetAwaiter().GetResult();
                var payment3DS = new Payment3DS()
                {
                    MD = "testorder-123",
                    PaReq = result.PaReq,
                    TermUrl = "http://locallocal"
                };
                var t = paymentService.AutoChallenge(payment3DS, result.Url)
                    .GetAwaiter()
                    .GetResult();
                var dataconfirm = new PaymentConfirmRequestData
                {
                    PaRes = t.Value,
                    TransactionId = result.TransactionId
                };
                var resultConfirm = paymentService.ConfirmPayment(dataconfirm).GetAwaiter().GetResult();
                Logger.LogInformation("Payment finished, status:" + resultConfirm.Status);
            }
            catch(PaymentException pex)
            {
                Logger.LogError("Payment exception. Status response:" + pex.StatusCode + " Content:" + pex.Content);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            
            serviceCollection.AddSingleton<IPaymentSettings>(new PaymentSettings()
            {
                MerchantId = "6fc3aa31-7afd-4df1-825f-192e60950ca1",
                SecretKey = "53cr3t",
                BaseAddress = new Uri("http://dummypay.host/api/")
            });
            serviceCollection.AddSingleton<IPaymentService, PaymentService>();
        }
    }
}
