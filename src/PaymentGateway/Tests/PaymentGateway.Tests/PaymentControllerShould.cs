using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.API.WriteAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class PaymentControllerShould
    {
        [Test]
        public async Task Create_payment_When_handling_PaymentRequest()
        {
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var eventSourcedRepository = new Repository<Payment>(new InMemoryEventStore(new FakeBus()));
            var controller = new PaymentRequestsController(eventSourcedRepository);
            var requestId = Guid.NewGuid();
            var paymentRequest = new PaymentRequest(requestId, "John Smith", "4524 4587 5698 1200", "05/19", new Money("EUR", 42.66), "321");

            IActionResult response = await controller.ProceedPaymentRequest(paymentRequest, guidGenerator);
            CheckThatPaymentResourceIsCorrectlyCreated(response, gatewayPaymentId, requestId);
        }

        [Test]
        public async Task Not_handle_a_PaymentRequest_more_than_once()
        {
        }

        private static void CheckThatPaymentResourceIsCorrectlyCreated(IActionResult response, Guid paymentId, Guid requestId)
        {
            Check.That(response).IsInstanceOf<CreatedAtActionResult>();
            var created = ((CreatedAtActionResult) response).Value;
            Check.That(created).IsInstanceOf<PaymentDto>();

            var payment = (PaymentDto) created;

            Check.That(payment.GateWayPaymentId).IsEqualTo(paymentId);
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.Status).IsEqualTo(PaymentStatus.Pending);
        }
    }
}