using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcquiringBanks.API;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using Polly;


namespace PaymentGateway.Tests
{
    public static class TestsUtils
    {
        public static PaymentRequest BuildPaymentRequest(Guid requestId)
        {
            return new PaymentRequest(requestId, "John Smith", "4524 4587 5698 1200", "05/19", new Money("EUR", 42.66),
                "321");
        }

        public static PaymentRequest BuildInvalidCardNumberPaymentRequest(Guid requestId)
        {
            return new PaymentRequest(requestId, "John Smith", "???? 4587 5698 1200", "05/19", new Money("EUR", 42.66),
                "321");
        }
    }


    [TestFixture]
    public class RequestPaymentShould
    {
        [Test]
        public async Task Create_payment_When_handling_PaymentRequest()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId);

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior());

            var response = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, new InMemoryPaymentIdsMapping(), cqrs.PaymentProcessor);

            CheckThatPaymentResourceIsCorrectlyCreated(response, gatewayPaymentId, requestId);
        }

        [Test]
        public async Task Not_handle_a_PaymentRequest_more_than_once()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior());

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);

            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult)actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Identical payment request will not be handled more than once");
        }

        [TestCase(AcquiringBanks.API.BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.Success)]
        [TestCase(AcquiringBanks.API.BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.RejectedByBank)]
        public async Task Return_proper_payment_status_When_AcquiringBank_accepts_or_reject_payment(BankPaymentStatus bankPaymentStatus, PaymentStatus expectedPaymentStatusReturnedByGateway)
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new AlwaysSuccessBankConnectionBehavior());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(expectedPaymentStatusReturnedByGateway);
            Check.That(payment.AcquiringBankPaymentId).IsEqualTo(bankPaymentId);
        }

        [Test]
        public async Task Return_PaymentFaulted_When_AcquiringBank_rejects_payment()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new SimulateGatewayException());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.FaultedOnGateway);
        }


        [TestCase(AcquiringBanks.API.BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.Success)]
        [TestCase(AcquiringBanks.API.BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.RejectedByBank)]
        public async Task Return_proper_payment_status_When_Connect_to_bank_fails_twice_then_connected_AND_AcquiringBank_accepts_or_reject_payment(BankPaymentStatus bankPaymentStatus, PaymentStatus expectedPaymentStatusReturnedByGateway)
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new FailTwiceBankThenSuccessConnectionBehavior());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(expectedPaymentStatusReturnedByGateway);
            Check.That(payment.AcquiringBankPaymentId).IsEqualTo(bankPaymentId);
        }

        [Test]
        public async Task Return_BankUnavailable_When_connection_to_bank_is_broken()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysFailBankConnectionBehavior(), new SimulateGatewayException());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.BankUnavailable);
        }


        [Test]
        public async Task Return_BadRequest_When_invalid_card_number_is_received()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildInvalidCardNumberPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysFailBankConnectionBehavior(), new SimulateGatewayException());
            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult)actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Invalid credit card number");

        }

        private static void CheckThatPaymentResourceIsCorrectlyCreated(IActionResult response, Guid paymentId,
            Guid requestId)
        {
            Check.That(response).IsInstanceOf<CreatedAtRouteResult>();
            var createdAtRouteResult = (CreatedAtRouteResult)response;

            var created = createdAtRouteResult.Value;
            Check.That(created).IsInstanceOf<PaymentDto>();
            var payment = (PaymentDto)created;

            Check.That(createdAtRouteResult.RouteValues["gateWayPaymentId"]).IsEqualTo(payment.GatewayPaymentId);


            Check.That(payment.GatewayPaymentId).IsEqualTo(paymentId);
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.Status).IsEqualTo(PaymentStatus.Requested);
        }
    }

    public class FailTwiceBankThenSuccessConnectionBehavior : IBankConnectionBehavior
    {
        private int _failed = 0;

        public async Task<bool> Connect()
        {
            while (_failed <= 2)
            {
                _failed++;
                throw new FailedConnectionToBankException();
            }

            return await Task.FromResult(true);
        }
    }
}