using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NFluent;
using NUnit.Framework;
using PaymentGateway.Infrastructure;
using PaymentGateway.Tests;

namespace PaymentGateway.Write.PerformanceTests
{
    [Ignore("Performance tests, may be time consuming. I want fast feedback using NCrunch. Launch this on demand")]
    public class WritePerformanceShould
    {
        [Test]
        public async Task Can_RequestPayments_concurrently()
        {
            await RequestPayments(1000);  
        }

        public static async Task RequestPayments(int concurrentClientsCount)
        {
            const string baseUri = "https://localhost:5001";
            var clients =  BuildHttpClients(concurrentClientsCount, baseUri);

            IEnumerable<Task<PaymentDto>> posts = clients.Select(async c =>
            {
                var paymentRequest = TestsUtils.BuildPaymentRequest(Guid.NewGuid());
                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await c.PostAsync("/api/PaymentRequests", content);

                Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

                Check.That(response.Headers.Location.ToString()).StartsWith($"{baseUri}/api/Payments/");

                var payment = JsonConvert.DeserializeObject<PaymentDto>(await response.Content.ReadAsStringAsync());
                Check.That(payment.Status).IsEqualTo(Domain.PaymentStatus.Requested);
                Check.That(payment.AcquiringBankPaymentId).IsEqualTo(Guid.Empty);

                //var gatewayPaymentId = payment.GatewayPaymentId;

                //while (true)
                //{
                //    var getPaymentResponse = await c.GetAsync($"/api/Payments/{gatewayPaymentId}");

                //    var polledPayment = JsonConvert.DeserializeObject<PaymentDto>(await getPaymentResponse.Content.ReadAsStringAsync());
                //    int polled = 0;
                //    if (polledPayment.Status == Domain.PaymentStatus.Requested)
                //    {
                //        polled++;
                //        await Task.Delay(TimeSpan.FromSeconds(1));
                //        continue;
                //    }

                //    Check.That(polledPayment.Status == Domain.PaymentStatus.RejectedByBank ||
                //               polledPayment.Status == Domain.PaymentStatus.Success ||
                //               polledPayment.Status == Domain.PaymentStatus.BankUnavailable
                //    ).IsTrue();

                //    Console.WriteLine($"Bank responds {polledPayment.Status} after polled {polled} seconds ");

                //    break;
                //}

                return payment;
            });

            await Task.WhenAll(posts);
        }

        public static IEnumerable<ISafeHttpClient> BuildHttpClients(int concurrentClientsCount, string baseUri)
        {
            
            var httpClientsContainer = new HttpClientsContainer();
            var client = httpClientsContainer.GetSafeHttpClient(baseUri);

            //var client = new HttpClient {BaseAddress = new Uri(baseUri)};
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return  Enumerable.Repeat(client, concurrentClientsCount);
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
                var paymentRequest = TestsUtils.BuildPaymentRequest(Guid.NewGuid());
                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return c.GetAsync("api/PaymentsDetails/f31499cc-ece3-4974-8785-1620d8c506f6");
            }); 

           foreach (var response in await Task.WhenAll(posts))
           {
               Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
           }
        }
    }
}