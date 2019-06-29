using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;


namespace PaymentGateway.Tests
{
    static class Utils
    {
        public static PaymentRequest BuildPaymentRequest(Guid requestId)
        {
            return new PaymentRequest(requestId, "John Smith", "4524 4587 5698 1200", "05/19", new Money("EUR", 42.66),
                "321");
        }
    }

    [TestFixture]
    public class RequestPaymentShould
    {
        private static void CheckThatPaymentResourceIsCorrectlyCreated(IActionResult response, Guid paymentId,
            Guid requestId)
        {
            Check.That(response).IsInstanceOf<CreatedAtRouteResult>();
            var createdAtRouteResult = (CreatedAtRouteResult) response;

            var created = createdAtRouteResult.Value;
            Check.That(created).IsInstanceOf<PaymentDto>();
            var payment = (PaymentDto) created;

            Check.That(createdAtRouteResult.RouteValues["gateWayPaymentId"]).IsEqualTo(payment.GateWayPaymentId);


            Check.That(payment.GateWayPaymentId).IsEqualTo(paymentId);
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.Status).IsEqualTo(PaymentStatus.Requested);
        }

        [Test]
        public async Task Create_payment_When_handling_PaymentRequest()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = Utils.BuildPaymentRequest(requestId);

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build( AcquiringBanks.API.BankPaymentStatus.Accepted );

            var response = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, new InMemoryPaymentIdsMapping(), cqrs.PaymentProcessor);

            CheckThatPaymentResourceIsCorrectlyCreated(response, gatewayPaymentId, requestId);
        }

        [Test]
        public async Task Not_handle_a_PaymentRequest_more_than_once()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = Utils.BuildPaymentRequest(requestId);

            var cqrs = await PaymentCQRS.Build( AcquiringBanks.API.BankPaymentStatus.Accepted );

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);

            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult) actionResult;
            var failDetail = (ProblemDetails) badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Identical payment request will not be handled more than once");
        }

        [Test]
        public async Task Return_payment_success_When_AcquiringBank_accepts_payment()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = Utils.BuildPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted);
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);

            //await cqrs.AcquiringBank.WaitForBankResponse();

            var payment = (await cqrs.ReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GateWayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.Success);
        }

        [Test]
        public async Task Returns_PaymentRejected_When_AcquiringBank_rejects_payment()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = Utils.BuildPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Rejected);
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);


            var payment = (await cqrs.ReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GateWayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.RejectedByBank);
        }


        [Test]
        public async Task Returns_PaymentFaulted_When_AcquiringBank_rejects_payment()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = Utils.BuildPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted, new SimulateException());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);


            var payment = (await cqrs.ReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GateWayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.FaultedOnGateway);
        }
    }
}