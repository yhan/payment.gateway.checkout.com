using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using PaymentGateway.Domain;
using PaymentGateway.Tests;

namespace Tests
{
    public class Tests
    {
        [Test]
        public async Task Can_RequestPayments_concurrently()
        {
            var client = new HttpClient {BaseAddress = new Uri("https://localhost:5002")};
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            IEnumerable<HttpClient> clients = Enumerable.Repeat(client, 1);

            //specify to use TLS 1.2 as default connection
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


            IEnumerable<Task<HttpResponseMessage>> posts = clients.Select(c =>
            {
                var paymentRequest = Utils.BuildPaymentRequest(Guid.NewGuid());
                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return c.PostAsync("/api/PaymentRequests", content);
            });


            HttpResponseMessage[] responses = await Task.WhenAll(posts);

            var contents = responses.Select(r =>
            {
                var httpContent = r.Content;
                //JsonConvert.DeserializeObject<>()

                return httpContent;
            });
        }
    }
}