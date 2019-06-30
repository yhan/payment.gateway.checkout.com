using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

                var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var paymentRequest = Utils.BuildPaymentRequest(Guid.NewGuid());
                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                try
                {
                    var response = client.GetAsync("api/PaymentsDetails/f31499cc-ece3-4974-8785-1620d8c506f6").Result;
                    var httpContent = response.Content;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                }
            }
        }
    }
}
