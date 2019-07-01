using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using NFluent;
using NUnit.Framework;  
using PaymentGateway.API;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.Tests;
using PaymentGateway.Write.PerformanceTests;

namespace PaymentGateway.Read.PerformanceTests
{

    [Ignore("Performance tests, may be time consuming. I want fast feedback using NCrunch. Launch this on demand")]
    public class ReadPerformanceShould
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            //await WritePerformanceShould.RequestPayments(100);
        }

        [Test]
        public async Task Get_PaymentDetails_After_Bank_has_accepted_or_rejected_payment() // m clients * n payments
        {
            const string baseUri = "https://localhost:5001";

            var clients = WritePerformanceShould.BuildHttpClients(50, baseUri).ToArray();

            var httpClient = clients.First();
            var response = await httpClient.GetAsync($"api/GatewayPaymentsIds");
            var gatewayPaymentsIds = JsonConvert.DeserializeObject<IEnumerable<Guid>>(await response.Content.ReadAsStringAsync());


            IEnumerable<Unit> units = Combine(clients, gatewayPaymentsIds);
            Parallel.ForEach(units, async u =>
            {
                var client = u.Client;
                var gatewayPaymentId = u.GatewayPaymentId;

                while (true)
                {
                    var payment = await Get<PaymentDto>(client, $"/api/Payments/{gatewayPaymentId}");
                    int polled = 0;
                    if (payment.Status == Domain.PaymentStatus.Requested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        polled++;
                        continue;
                    }

                    Check.That(payment.Status == Domain.PaymentStatus.RejectedByBank ||
                               payment.Status == Domain.PaymentStatus.Success ||
                               payment.Status == Domain.PaymentStatus.BankUnavailable
                    ).IsTrue();

                    Console.WriteLine($"Bank responds {payment.Status} after polled {polled} seconds ");

                    if (payment.Status == Domain.PaymentStatus.RejectedByBank ||
                        payment.Status == Domain.PaymentStatus.Success)
                    {
                        var details =
                            await Get<PaymentDetailsDto>(client, $"/api/PaymentsDetails/{payment.AcquiringBankPaymentId}");

                        Check.That(details.AcquiringBankPaymentId).IsEqualTo(payment.AcquiringBankPaymentId);
                        Check.That(details.Status == Domain.PaymentStatus.RejectedByBank || details.Status == Domain.PaymentStatus.Success  ).IsTrue();
                    }

                    break;
                }

            });

        }


        public static  async Task<T> Get<T>(ISafeHttpClient client, string getUri)
        {
            var response = await client.GetAsync(getUri);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }


        public static IEnumerable<Unit> Combine(ISafeHttpClient[] clients, IEnumerable<Guid> gatewayPaymentsIds)
        {
            foreach (var client in clients)
            {
                foreach (var gatewayPaymentsId in gatewayPaymentsIds)
                {
                    yield return new Unit(client, gatewayPaymentsId);
                }
            }
        }
    }

    public class Unit
    {
        public ISafeHttpClient Client { get; }
        public Guid GatewayPaymentId { get; }

        public Unit(ISafeHttpClient client, Guid gatewayPaymentId)
        {
            Client = client;
            GatewayPaymentId = gatewayPaymentId;
        }
    }
}