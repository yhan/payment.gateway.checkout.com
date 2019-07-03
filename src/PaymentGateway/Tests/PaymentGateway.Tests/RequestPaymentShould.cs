using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NUnit.Framework;
using PaymentGateway;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;


namespace PaymentGateway.Tests
{
    [TestFixture]
    public class RequestPaymentShould
    {
        [Test]
        public async Task Create_payment_When_handling_PaymentRequest()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Apple);

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior());

            var response = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, new PaymentRequestsMemory(), cqrs.PaymentProcessor);

            CheckThatPaymentResourceIsCorrectlyCreated(response, gatewayPaymentId, requestId);
        }

        [Test]
        public async Task Not_handle_a_PaymentRequest_more_than_once()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior());

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);

            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult)actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Identical payment request will not be handled more than once");
        }

        [TestCase(BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.Success)]
        [TestCase(BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.RejectedByBank)]
        public async Task Return_proper_payment_status_When_AcquiringBank_accepts_or_reject_payment(BankPaymentStatus bankPaymentStatus, PaymentStatus expectedPaymentStatusReturnedByGateway)
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Apple);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new AlwaysSuccessBankConnectionBehavior());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(expectedPaymentStatusReturnedByGateway);
            Check.That(payment.Approved).IsEqualTo(payment.Status == PaymentGateway.Domain.PaymentStatus.Success);
            Check.That(payment.AcquiringBankPaymentId).IsEqualTo(bankPaymentId);
        }

        [Test]
        public async Task Return_PaymentFaulted_When_AcquiringBank_rejects_payment()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new SimulateGatewayException());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.FaultedOnGateway);
            Check.That(payment.Approved.Value).IsFalse();
            Check.That(payment.AcquiringBankPaymentId).IsNull();
        }


        [TestCase(BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.Success)]
        [TestCase(BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.RejectedByBank)]
        public async Task Return_proper_payment_status_When_Connect_to_bank_fails_twice_then_connected_AND_AcquiringBank_accepts_or_reject_payment(BankPaymentStatus bankPaymentStatus, PaymentStatus expectedPaymentStatusReturnedByGateway)
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new FailTwiceBankThenSuccessConnectionBehavior());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(expectedPaymentStatusReturnedByGateway);
            Check.That(payment.Approved).IsEqualTo(payment.Status == PaymentGateway.Domain.PaymentStatus.Success);
            Check.That(payment.AcquiringBankPaymentId).IsEqualTo(bankPaymentId);
        }

        [Test]
        public async Task Return_BankUnavailable_When_connection_to_bank_is_broken()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Apple);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysFailBankConnectionBehavior(), new SimulateGatewayException());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);
            Check.That(payment.Approved.Value).IsFalse();
            Check.That(payment.Status).IsEqualTo(PaymentStatus.BankUnavailable);
            Check.That(payment.AcquiringBankPaymentId).IsNull();
        }


        [TestCase("456A 4589 1052 4568")]
        [TestCase("4560 ???9 1052 4568")]
        [TestCase("45601199 1052 4568")]
        [TestCase("45601199 1052 4568 ")]
        public async Task Return_BadRequest_When_invalid_card_number_is_received(string invalidCardNumber)
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildInvalidCardNumberPaymentRequest(requestId, invalidCardNumber);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new SimulateGatewayException());
            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult)actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Invalid card number");
        }

        [TestCase("a45")]
        public async Task Return_BadRequest_When_invalid_card_cvv_is_received(string invalidCvv)
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildInvalidCardCvvPaymentRequest(requestId, invalidCvv);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new SimulateGatewayException());
            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult)actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Invalid card CVV");
        }


        [TestCaseSource("InvalidPaymentRequests")]//
        public async Task Return_BadRequest_When_PaymenRequest_is_not_well_formed(PaymentRequest invalidRequest, string expectedErrorMessage)
        {
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new SimulateGatewayException());
            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(invalidRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult)actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo(expectedErrorMessage);

            Check.That(await cqrs.PaymentRequestsMemory.AlreadyHandled(new PaymentRequestId(invalidRequest.RequestId)))
                .IsFalse();
        }

        public static IEnumerable InvalidPaymentRequests
        {
            get
            {
                var validMoney = new Money("CNY", 42);
                var validCard = new Infrastructure.Card("1234 5623 5489 1004", "05/19", "123");
                var systemNotAwareOfThisMerchant = Guid.NewGuid();

                yield return new TestCaseData(new PaymentRequest(requestId: Guid.Empty, merchantId: Guid.NewGuid(), amount: validMoney, card: validCard), "Invalid Request id missing");
                yield return new TestCaseData(new PaymentRequest(Guid.NewGuid(), merchantId: Guid.Empty, amount: validMoney, card: validCard), "Merchant id missing");
                yield return new TestCaseData(new PaymentRequest(Guid.NewGuid(), MerchantsRepository.Amazon, amount: null, card: validCard), "Amount missing");
                yield return new TestCaseData(new PaymentRequest(Guid.NewGuid(), MerchantsRepository.Amazon, amount: validMoney, card: null), "Card info missing");
                yield return new TestCaseData(new PaymentRequest(Guid.NewGuid(), MerchantsRepository.Amazon, amount: new Money("helloworld", 42), card: validCard), "Currency is absent or not correctly formatted");
                yield return new TestCaseData(new PaymentRequest(Guid.NewGuid(), merchantId: MerchantsRepository.Amazon, amount: new Money("USD", -42), card: validCard), "Amount should be greater than 0");
                yield return new TestCaseData(new PaymentRequest(Guid.NewGuid(), merchantId: systemNotAwareOfThisMerchant, amount: new Money("USD", 42), card: validCard), $"Merchant {systemNotAwareOfThisMerchant} has not been onboarded");
            }
        }
        
        [TestCase("13/12")]
        [TestCase("112")]
        [TestCase("aaa")]
        [TestCase("aa/ba")]
        public async Task Return_BadRequest_When_invalid_card_expiry_is_received(string invalidExpiry)
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildInvalidCardExpiryPaymentRequest(requestId, invalidExpiry);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new SimulateGatewayException());
            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult)actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Invalid card expiry");
        }

        private static void CheckThatPaymentResourceIsCorrectlyCreated(IActionResult response, Guid paymentId,
            Guid requestId)
        {
            Check.That(response).IsInstanceOf<AcceptedAtRouteResult>();
            var createdAtRouteResult = (AcceptedAtRouteResult)response;

            var created = createdAtRouteResult.Value;
            Check.That(created).IsInstanceOf<PaymentDto>();
            var payment = (PaymentDto)created;

            Check.That(createdAtRouteResult.RouteValues["gateWayPaymentId"]).IsEqualTo(payment.GatewayPaymentId);

            Check.That(payment.GatewayPaymentId).IsEqualTo(paymentId);
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.Status).IsEqualTo(PaymentStatus.Pending);
            Check.That(payment.Approved).IsNull();
        }
    }

    public class FailTwiceBankThenSuccessConnectionBehavior : IConnectToAcquiringBanks
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