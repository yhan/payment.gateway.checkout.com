using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.Tests;

namespace PaymentGateway.PerformanceTests
{
    [Ignore("Performance tests, may be time consuming. I want fast feedback using NCrunch. Launch this on demand")]
    public class PerformanceShould
    {
        [Test]
        public async Task Can_RequestPayments_concurrently()
        {
            const string baseUri = "https://localhost:5001";
            var client = new HttpClient {BaseAddress = new Uri(baseUri)};
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            var clients = Enumerable.Repeat(client, 100);

            IEnumerable<Task<bool>> posts = clients.Select(async c =>
            {
                var paymentRequest = Utils.BuildPaymentRequest(Guid.NewGuid());
                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await  c.PostAsync("/api/PaymentRequests", content);

                Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

                Check.That(response.Headers.Location.ToString()).StartsWith($"{baseUri}/api/Payments/");

                var payment = JsonConvert.DeserializeObject<PaymentDto>(await response.Content.ReadAsStringAsync());
                Check.That(payment.Status).IsEqualTo(Domain.PaymentStatus.Requested);
                Check.That(payment.AcquiringBankPaymentId).IsEqualTo(Guid.Empty);

                var gatewayPaymentId = payment.GatewayPaymentId;

                int polled = 0;
                while (true)
                {
                    var getPaymentResponse = await c.GetAsync($"/api/Payments/{gatewayPaymentId}");

                    var polledPayment = JsonConvert.DeserializeObject<PaymentDto>(await getPaymentResponse.Content.ReadAsStringAsync());
                    if (polledPayment.Status == Domain.PaymentStatus.Requested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        polled++;
                        continue;
                    }

                    Check.That(polledPayment.Status == Domain.PaymentStatus.RejectedByBank ||
                               polledPayment.Status == Domain.PaymentStatus.Success ||
                               polledPayment.Status == Domain.PaymentStatus.BankUnavailable
                               ).IsTrue();

                    Console.WriteLine($"Bank responds {polledPayment.Status} after polled {polled} seconds ");

                    break;
                }

                return true;
            });

            await Task.WhenAll(posts);
        }

        [Test]
        public async Task Always_returns_NotFound_When_try_to_get_non_existing_PaymentDetails()
        {
            var client = new HttpClient {BaseAddress = new Uri("https://localhost:5001")};
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            var clients = Enumerable.Repeat(client, 1);

            var posts = clients.Select(c =>
            {
                var paymentRequest = Utils.BuildPaymentRequest(Guid.NewGuid());
                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return c.GetAsync("api/PaymentsDetails/f31499cc-ece3-4974-8785-1620d8c506f6");
            }); 

           foreach (var response in await Task.WhenAll(posts))
           {
               Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
           }
        }


        [Test]
        [Repeat(1000)]
        public void Con()
        {
            var b = new ConcurrentBag<int> {1, 2};

            var array = b.ToArray();

            Check.That(array[0]).IsEqualTo(2);
            Check.That(array[1]).IsEqualTo(1);

            b.TryTake(out var p);
            Console.WriteLine(p);
            Check.That(p).IsEqualTo(2);


            
            b.TryTake(out var q);
            Console.WriteLine(q);
            Check.That(q).IsEqualTo(1);
        }
    }
}