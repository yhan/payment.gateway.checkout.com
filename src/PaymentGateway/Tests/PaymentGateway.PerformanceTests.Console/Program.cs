using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NFluent;
using PaymentGateway.Infrastructure;
using PaymentGateway.Read.PerformanceTests;
using PaymentGateway.Tests;
using PaymentGateway.Write.PerformanceTests;

namespace PaymentGateway.PerformanceTests.Console
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        public static async Task ConcurrentClientsRequestsPaymentThenReadPaymentDetails()
        {

            while (true)
            {
                var key = System.Console.ReadKey();
                if (key.Key == ConsoleKey.W)
                {
                    System.Console.WriteLine();

                    var concurrentClientsCount = int.Parse(System.Console.ReadLine());

                    RequestPayments(concurrentClientsCount).Wait();
                }
                else
                {
                    while (true)
                    {
                        System.Console.WriteLine();

                        var concurrentClientsCount = int.Parse(System.Console.ReadLine());
                        const string baseUri = "https://localhost:5001";

                        var clients = WritePerformanceShould.BuildHttpClients(concurrentClientsCount, baseUri).ToArray();

                        var httpClient = clients.First();
                        var gatewayPaymentsIds = await Get<IEnumerable<Guid>>(httpClient, $"api/GatewayPaymentsIds");

                        var bankRespondedPaymentsCount = (await Get<IEnumerable<Guid>>(httpClient, $"api/GatewayPaymentsIds")).Count();

                        IEnumerable<Unit> units = ReadPerformanceShould.Combine(clients, gatewayPaymentsIds);

                        var stopwatch = Stopwatch.StartNew();

                        var parallelLoopResult = Parallel.ForEach(units, async u =>
                        {
                            var client = u.Client;
                            var gatewayPaymentId = u.GatewayPaymentId;

                            while (true)
                            {
                                PaymentDto payment;

                                try
                                {
                                    payment = await Get<PaymentDto>(client, $"/api/Payments/{gatewayPaymentId}");
                                }
                                catch (Exception e)
                                {
                                    System.Console.WriteLine(e);
                                    break;
                                }
                                int polled = 0;
                                if (payment.Status == Domain.PaymentStatus.Pending)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                    polled++;
                                    continue;
                                }

                                Check.That(payment.Status == Domain.PaymentStatus.RejectedByBank ||
                                           payment.Status == Domain.PaymentStatus.Success ||
                                           payment.Status == Domain.PaymentStatus.BankUnavailable
                                ).IsTrue();

                                System.Console.WriteLine($"Bank responds {payment.Status} after polled {polled} seconds ");

                                if (payment.Status == Domain.PaymentStatus.RejectedByBank ||
                                    payment.Status == Domain.PaymentStatus.Success)
                                {
                                    var details =
                                        await Get<PaymentDetailsDto>(client, $"/api/PaymentsDetails/{payment.AcquiringBankPaymentId}");

                                    Check.That(details.AcquiringBankPaymentId).IsEqualTo(payment.AcquiringBankPaymentId);
                                    Check.That(details.Status == Domain.PaymentStatus.RejectedByBank || details.Status == Domain.PaymentStatus.Success).IsTrue();
                                }

                                break;
                            }

                        });

                        Check.That(parallelLoopResult.IsCompleted).IsTrue();
                        System.Console.WriteLine($"Finishes: {stopwatch.Elapsed} Concurrent clients: {concurrentClientsCount}, payments: {bankRespondedPaymentsCount}");
                    }
                }

            }
        }

        public static async Task RequestPayments(int concurrentClientsCount)
        {
            const string baseUri = "https://localhost:5001";
            var clients = WritePerformanceShould.BuildHttpClients(concurrentClientsCount, baseUri);

            IEnumerable<Task<PaymentDto>> posts = clients.Select(async c =>
            {
                var paymentRequest = TestsUtils.BuildPaymentRequest(Guid.NewGuid(), MerchantsRepository.Apple);
                var content = new StringContent(JsonConvert.SerializeObject(paymentRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await c.PostAsync("/api/PaymentRequests", content);

                Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

                Check.That(response.Headers.Location.ToString()).StartsWith($"{baseUri}/api/Payments/");

                var payment = JsonConvert.DeserializeObject<PaymentDto>(await response.Content.ReadAsStringAsync());
                Check.That(payment.Status).IsEqualTo(Domain.PaymentStatus.Pending);
                Check.That(payment.AcquiringBankPaymentId).IsEqualTo(Guid.Empty);

                return payment;
            });

            await Task.WhenAll(posts);
        }


        static void Main(string[] args)
        {
            ConcurrentClientsRequestsPaymentThenReadPaymentDetails().Wait();
        }

        private static async Task<T> Get<T>(ISafeHttpClient client, string getUri)
        {
            var response = await client.GetAsync(getUri);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}
