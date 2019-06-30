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
using PaymentGateway.API;
using PaymentGateway.Tests;

namespace PaymentGateway.PerformanceTests
{
    public class PerformanceShould
    {
        [Test]
        public async Task Can_RequestPayments_concurrently()
        {
            const string baseUri = "https://localhost:5001";
            var client = new HttpClient {BaseAddress = new Uri(baseUri)};
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            var clients = Enumerable.Repeat(client, 1);

            var posts = clients.Select(c =>
            {
                var paymentRequest = Utils.BuildPaymentRequest(Guid.NewGuid());
                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return c.PostAsync("/api/PaymentRequests", content);
            });
            
            foreach (var response in await Task.WhenAll(posts))
            {
                Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

                Check.That(response.Headers.Location.ToString()).StartsWith($"{baseUri}/api/Payments/");

                var payment = JsonConvert.DeserializeObject<PaymentDto>(await response.Content.ReadAsStringAsync());
                Check.That(payment.Status).IsEqualTo(Domain.PaymentStatus.Requested);
                Check.That(payment.AcquiringBankPaymentId).IsEqualTo(Guid.Empty);

            }
        }

        [Test]
        public async Task Can_GetPaymentsDetails_concurrently_When_Payments_do_not_exist()
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
    }
}