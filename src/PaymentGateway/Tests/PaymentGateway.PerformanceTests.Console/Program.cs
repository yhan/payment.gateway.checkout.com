using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NFluent;
using PaymentGateway.API;
using PaymentGateway.Tests;

namespace PaymentGateway.PerformanceTests.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                System.Console.ReadKey();

                const string baseUri = "https://localhost:5001";
                var client = new HttpClient { BaseAddress = new Uri(baseUri) };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var clients = Enumerable.Repeat(client, 10);

                IEnumerable<Task<bool>> posts = clients.Select(async c =>
                {
                    var paymentRequest = Utils.BuildPaymentRequest(Guid.NewGuid());
                    var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await c.PostAsync("/api/PaymentRequests", content);

                    Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

                    Check.That(response.Headers.Location.ToString()).StartsWith($"{baseUri}/api/Payments/");

                    var payment = JsonConvert.DeserializeObject<PaymentDto>(await response.Content.ReadAsStringAsync());
                    Check.That(payment.Status).IsEqualTo(Domain.PaymentStatus.Requested);
                    Check.That(payment.AcquiringBankPaymentId).IsEqualTo(Guid.Empty);

                    var gatewayPaymentId = payment.GatewayPaymentId;

                    while (true)
                    {
                        var getPaymentResponse = await c.GetAsync($"/api/Payments/{gatewayPaymentId}");

                        var polledPayment = JsonConvert.DeserializeObject<PaymentDto>(await getPaymentResponse.Content.ReadAsStringAsync());
                        if (polledPayment.Status == Domain.PaymentStatus.Requested)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            continue;
                        }

                        Check.That(polledPayment.Status == Domain.PaymentStatus.RejectedByBank ||
                                   polledPayment.Status == Domain.PaymentStatus.Success).IsTrue();
                        break;
                    }

                    return true;
                });

                Task.WhenAll(posts).Wait();

            }
        }
    }
}
