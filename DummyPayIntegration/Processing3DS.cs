using DummyPayIntegration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DummyPayIntegration
{
    internal class Processing3DS
    {
        public ILogger Logger { get; }
        public IPaymentSettings paymentSettings { get; }

        public Processing3DS(ILoggerFactory loggerFactory, IPaymentSettings paymentSettings)
        {
            Logger = loggerFactory?.CreateLogger<RequestingPayment>();
            if (Logger == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            this.paymentSettings = paymentSettings;
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
            string name="", type = "", value = "";
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
    }
}

