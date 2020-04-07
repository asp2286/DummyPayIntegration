using System;
using System.IO;
using System.Threading.Tasks;
using DummyPayIntegration;
using DummyPayIntegration.Domain;
using DummyPayIntegration.Models;
using DummyPayIntegration.Service;
using Microsoft.Extensions.Configuration;
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
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            ConfigureServices(serviceCollection, config["MerchantId"], config["SecretKey"], config["BaseAddress"]);
            Services = serviceCollection.BuildServiceProvider();
            Logger = Services.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<Application>();
            Logger.LogInformation("Application created successfully.");
            var paymentService = Services.GetRequiredService<IDummyService>();
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
                var result = paymentService.CreatePayment(data, config["MD"], config["TermUrl"]).GetAwaiter().GetResult();
                Logger.LogInformation("Payment finished, status:" + result.Status);
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

        private void ConfigureServices(IServiceCollection serviceCollection, string merchant, string secret, string baseAddr)
        {
            
            serviceCollection.AddSingleton<IPaymentSettings>(new PaymentSettings()
            {
                MerchantId = merchant,
                SecretKey = secret,
                BaseAddress = new Uri(baseAddr)
            });
            serviceCollection.AddSingleton<IDummyModel, DummyModel>();
            serviceCollection.AddSingleton<IDummyDomain, DummyDomain>();
            serviceCollection.AddSingleton<IDummyService, DummyService>();
        }
    }
}
