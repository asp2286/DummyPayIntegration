using DummyPayIntegration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DummyPayIntegration.Domain
{
    public class DummyDomain : IDummyDomain
    {
		public ILogger Logger { get; }

        public IPaymentSettings paymentSettings { get; }

        public IDummyModel dummyModel { get; }

        public DummyDomain (ILoggerFactory loggerFactory, IDummyModel dummyModel, IPaymentSettings paymentSettings)
        {
			Logger = loggerFactory?.CreateLogger<DummyDomain>();
			if (Logger == null)
			{
				throw new ArgumentNullException(nameof(loggerFactory));
			}
            this.paymentSettings = paymentSettings;
            this.dummyModel = dummyModel;
		}

        public async Task<PaymentCreationResultData> PaymentRequest(PaymentCreationRequestData requestData)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(requestData);
            if (!Validator.TryValidateObject(requestData, context, results, true))
            {
                foreach (var error in results)
                {
                    Logger.LogError(error.ErrorMessage);
                }
                throw new Exception("Validation failed");
            }
            if (!Luhn(requestData.CardNumber))
            {
                Logger.LogError("The card number is invalid");
                throw new Exception("Validation failed");
            }

            var data = await dummyModel.PaymentRequest(requestData);
            if (string.IsNullOrEmpty(data.Type))
            {
                return data.Result;
            }
            else
            {
                throw new Exception("Error request. Type=" + data.Type + " Message=" + data.Message);
            }
        }

        public async Task<KeyValuePair<string, string>> AutoChallenge(Payment3DS payment3DS, string url)
        {
            var kvp = GetKeyValuePairs(payment3DS);
            var content = await dummyModel.GetFormContent(kvp, url);
            var nvc = ParseFormForRequest(content.Value);
            var res = await dummyModel.Send3DSConfirm(content.Key, nvc, url);

            return res;
        }

        public List<KeyValuePair<string, string>> GetKeyValuePairs(Payment3DS payment3DS)
        {
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("MD", payment3DS.MD));
            nvc.Add(new KeyValuePair<string, string>("PaReq", payment3DS.PaReq));
            nvc.Add(new KeyValuePair<string, string>("TermUrl", payment3DS.TermUrl));
            return nvc;
        }

        public List<KeyValuePair<string, string>> ParseFormForRequest(string html)
        {
            var nvc = new List<KeyValuePair<string, string>>();
            Regex reHref = new Regex(@"(type|name|value|action)=""(.*?)""", RegexOptions.Multiline);
            string name = "", type = "", value = "";
            foreach (Match match in reHref.Matches(html).OrderBy(x => x.Index))
            {
                var matchValue = match.Value.Substring(match.Value.IndexOf('"')).Replace("\"", "");
                switch (match.Value.Substring(0, match.Value.IndexOf('=')))
                {
                    case "type":
                        if (matchValue != "submit")
                        {
                            type = matchValue;
                        }
                        break;

                    case "name":
                        if (matchValue != "Code")
                        {
                            name = matchValue;
                        }
                        break;

                    case "value":
                        if (matchValue != "Send")
                        {
                            value = matchValue;
                        }
                        break;
                    case "action":
                        nvc.Add(new KeyValuePair<string, string>("action", matchValue));
                        break;
                }
                if (name != "" && value != "")
                {
                    nvc.Add(new KeyValuePair<string, string>(name, value));
                    name = "";
                    value = "";
                }
            }
            nvc.Add(new KeyValuePair<string, string>("Code", "12345678"));
            return nvc;
        }

        public static bool Luhn(string digits)
		{
			return digits.All(char.IsDigit) && digits.Reverse()
				.Select(c => c - 48)
				.Select((thisNum, i) => i % 2 == 0
					? thisNum
					: ((thisNum *= 2) > 9 ? thisNum - 9 : thisNum)
				).Sum() % 10 == 0;
		}

        public async Task<PaymentConfirmResultData> PaymentConfirm(PaymentConfirmRequestData requestData)
        {
            var data = await dummyModel.PaymentConfirm(requestData);
            if (string.IsNullOrEmpty(data.Type))
            {
                return data.Result;
            }
            else
            {
                throw new Exception("Error request. Type=" + data.Type + " Message=" + data.Message);
            }
        }
    }
}
