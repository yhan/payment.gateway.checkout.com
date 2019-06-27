using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.API.Controllers;
using PaymentGateway.Domain;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class PaymentControllerShould
    {
        [Test]
        public async Task Create_payment_When_handling_PaymentRequest()
        {
            var paymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(paymentId);

            var controller = new PaymentRequestsController();
            var requestId = Guid.NewGuid();
            var paymentRequest = new PaymentRequest(requestId, "John Smith", "4524 4587 5698 1200", "05/19", new Money("EUR", 42.66), "321");

            IActionResult response = await controller.ProceedPaymentRequest(paymentRequest, guidGenerator);
            CheckThatPaymentResourceIsCorrectlyCreated(response, paymentId, requestId);
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