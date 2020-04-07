using DummyPayIntegration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

namespace DummyPayIntegration.Models
{
    public class DummyModel : IDummyModel
    {
        public ILogger Logger { get; }

        public IPaymentSettings paymentSettings { get; }

        public DummyModel(ILoggerFactory loggerFactory, IPaymentSettings paymentSettings)
        {
            Logger = loggerFactory?.CreateLogger<DummyModel>();
            if (Logger == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            this.paymentSettings = paymentSettings;
        }

        public async Task<PaymentCreationResponseData> PaymentRequest(PaymentCreationRequestData requestData)
        {
            using (var httpClient = new HttpClient { BaseAddress = paymentSettings.BaseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Mechant-Id", paymentSettings.MerchantId);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Secret-Key", paymentSettings.SecretKey);

                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(requestData, serializerOptions);
                var jsonContent = new StringContent(json, System.Text.Encoding.Default, "application/json");
                Logger.LogInformation("JSON DATA:" + json);
                Logger.LogInformation("Send request to server...");
                try
                {
                    var contentType = "application/json";
                    using (var response = await httpClient.PostAsync("payment/create", jsonContent))
                    {

                        var responseData = await response.Content.ReadAsStringAsync();
                        Logger.LogInformation("Received request from server");
                        Logger.LogInformation("RESPONSE DATA:" + responseData);
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.BadRequest:
                                if (response.Content.Headers.ContentType == new System.Net.Http.Headers.MediaTypeHeaderValue(contentType))
                                {
                                    var error = JsonSerializer.Deserialize<PaymentCreationResponseData>(responseData);
                                    return error;
                                }
                                else
                                {
                                    throw new PaymentException
                                    {
                                        StatusCode = (int)response.StatusCode,
                                        Content = responseData
                                    };
                                }

                            case HttpStatusCode.Unauthorized:
                                throw new PaymentException
                                {
                                    StatusCode = (int)response.StatusCode,
                                    Content = responseData
                                };

                            case HttpStatusCode.OK:
                                var deserializerOptions = new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                    WriteIndented = true
                                };
                                var result = JsonSerializer.Deserialize<PaymentCreationResponseData>(responseData, deserializerOptions);
                                return result;

                            default:
                                throw new PaymentException
                                {
                                    StatusCode = (int)response.StatusCode,
                                    Content = responseData
                                };
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task<KeyValuePair<Cookie, string>> GetFormContent(List<KeyValuePair<string, string>> kvp, string url)
        {
            var cookieContainer = new CookieContainer();
            var uri = new Uri(url);
            using (var httpClientHandler = new HttpClientHandler
            {
                CookieContainer = cookieContainer
            })
            {
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    Logger.LogInformation("Send secure request to server...");
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, url)
                        {
                            Content = new FormUrlEncodedContent(kvp)
                        };
                        using (var response = await httpClient.SendAsync(request))
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            Logger.LogInformation("Received request from server");
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    var cookie = cookieContainer.GetCookies(uri)[0];
                                    return new KeyValuePair<Cookie, string>(cookie, responseData);

                                default:
                                    throw new PaymentException
                                    {
                                        StatusCode = (int)response.StatusCode,
                                        Content = responseData
                                    };
                            }
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// return KeyValuePair Key=MD, Value=PARes
        /// </summary>
        /// <param name="cookie">Site cookie</param>
        /// <param name="kvp"></param>
        /// <param name="lastUrl"></param>
        /// <returns></returns>
        public async Task<KeyValuePair<string,string>> Send3DSConfirm(Cookie cookie, List<KeyValuePair<string, string>> kvp, string lastUrl)
        {
            var cookieContainer = new CookieContainer();
            var action = kvp.Where(c => c.Key == "action").FirstOrDefault();
            var url = action.Value;
            kvp.Remove(action);
            var baseUrl = new Uri(lastUrl).GetLeftPart(UriPartial.Authority);
            using (var httpClientHandler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false
            })
            {
                using (var httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri(baseUrl) })
                {
                    Logger.LogInformation("Send secure request to server...");
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, url)
                        {
                            Content = new FormUrlEncodedContent(kvp)
                        };
                        cookieContainer.Add(cookie);
                        using (var response = await httpClient.SendAsync(request))
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            Logger.LogInformation("Received request from server");
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.Redirect:
                                    var location = response.Headers.Location.ToString();
                                    var parameters = HttpUtility.ParseQueryString(location);
                                    return new KeyValuePair<string, string>(parameters["MD"], parameters["PARes"]);

                                default:
                                    throw new PaymentException
                                    {
                                        StatusCode = (int)response.StatusCode,
                                        Content = responseData
                                    };
                            }
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
        }

        public async Task<PaymentConfirmResponseData> PaymentConfirm(PaymentConfirmRequestData requestData)
        {
            using (var httpClient = new HttpClient { BaseAddress = paymentSettings.BaseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Mechant-Id", paymentSettings.MerchantId);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Secret-Key", paymentSettings.SecretKey);

                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(requestData, serializerOptions);
                var jsonContent = new StringContent(json, System.Text.Encoding.Default, "application/json");
                Logger.LogInformation("JSON DATA:" + json);
                Logger.LogInformation("Send request to server...");
                try
                {
                    var contentType = "application/json";
                    using (var response = await httpClient.PostAsync("payment/confirm", jsonContent))
                    {

                        var responseData = await response.Content.ReadAsStringAsync();
                        Logger.LogInformation("Received request from server");
                        Logger.LogInformation("RESPONSE DATA:" + responseData);
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.BadRequest:
                                if (response.Content.Headers.ContentType == new System.Net.Http.Headers.MediaTypeHeaderValue(contentType))
                                {
                                    var error = JsonSerializer.Deserialize<PaymentConfirmResponseData>(responseData);
                                    return error;
                                }
                                else
                                {
                                    throw new PaymentException
                                    {
                                        StatusCode = (int)response.StatusCode,
                                        Content = responseData
                                    };
                                }

                            case HttpStatusCode.Unauthorized:
                                throw new PaymentException
                                {
                                    StatusCode = (int)response.StatusCode,
                                    Content = responseData
                                };

                            case HttpStatusCode.OK:
                                var deserializerOptions = new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                    WriteIndented = true
                                };
                                var result = JsonSerializer.Deserialize<PaymentConfirmResponseData>(responseData, deserializerOptions);
                                return result;

                            default:
                                throw new PaymentException
                                {
                                    StatusCode = (int)response.StatusCode,
                                    Content = responseData
                                };
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
    }
}
