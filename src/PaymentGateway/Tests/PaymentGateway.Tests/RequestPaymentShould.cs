using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class PaymentRequestValidationShould
    {

        [Test, Combinatorial]
        public void Detect_malformatted_PaymentRequest(
            [Values("456A 4589 1052 4568", "4560 ???9 1052 4568", "45601199 1052 4568", "45601199 1052 4568 ")]string cardNumber,
            [Values("05/19", "13/12", "112", "aaa", "aa/ba")]string expiry, 
            [Values( "a45", "789456123")]string cvv, 
            [Values( "EU", "USDUSD")]string currency, 
            [Values( -42, 0)]double value)
        {
            var card = new Infrastructure.Card(cardNumber, expiry, cvv);
            var paymentRequest = new PaymentGateway.Infrastructure.PaymentRequest(Guid.Empty, Guid.Empty, amount: new Money(currency, value), card: card);
            var validationResults = paymentRequest.Validate(null);

            Check.That(validationResults.Select(x =>x.ErrorMessage))
                .IsOnlyMadeOf("Request id missing", 
                    "Merchant id missing", 
                    "Invalid card number", 
                    "Invalid card CVV", 
                    "Invalid card expiry",
                    "Amount should be greater than 0", 
                    "Currency is absent or not correctly formatted");
        }

        [Test]
        public void Consider_as_valid_paymentRequest()
        {
            var card = new Infrastructure.Card("4564 4589 1052 4568", "05/22", "123");
            var paymentRequest = new PaymentGateway.Infrastructure.PaymentRequest(Guid.NewGuid(), Guid.NewGuid(), amount: new Money("EUR", 42), card: card);
            var validationResults = paymentRequest.Validate(null);
            Check.That(validationResults).IsEmpty();
        }
    }

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

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)));

            var response = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, new PaymentRequestsMemory(), cqrs.PaymentProcessor);

            CheckThatPaymentResourceIsCorrectlyCreated(response, gatewayPaymentId, requestId);
        }

        [Test]
        public async Task Not_handle_a_PaymentRequest_more_than_once()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)));

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
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)));
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

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), new SimulateGatewayException());
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
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new FailTwiceBankThenSuccessConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)));
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

            var delayProviderForTesting = new DelayProviderForTesting(TimeSpan.FromMilliseconds(1));
            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysFailBankConnectionBehavior(), delayProviderForTesting, new SimulateGatewayException());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);
            Check.That(payment.Approved.Value).IsFalse();
            Check.That(payment.Status).IsEqualTo(PaymentStatus.BankUnavailable);
            Check.That(payment.AcquiringBankPaymentId).IsNull();
        }


        [Test]
        public async Task Return_BadRequest_When_PaymenRequest_Contains_a_merchant_not_onboarded_yet()
        {
            var validCard = new Infrastructure.Card("1234 5623 5489 1004", "05/19", "123");
            var systemNotAwareOfThisMerchant = Guid.NewGuid();

            var invalidRequest = new PaymentRequest(Guid.NewGuid(), merchantId: systemNotAwareOfThisMerchant,
                amount: new Money("USD", 42), card: validCard);

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), new SimulateGatewayException());
            var actionResult = await cqrs.RequestsController.ProceedPaymentRequest(invalidRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult)actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo($"Merchant {systemNotAwareOfThisMerchant} has not been onboarded");

            Check.That(await cqrs.PaymentRequestsMemory.AlreadyHandled(new PaymentRequestId(invalidRequest.RequestId)))
                .IsFalse();
        }

        [Test]
        public async Task Return_InternalServerError_When_Processing_PaymentRequest_timeout()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(bankPaymentId), 
                new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(400)));
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);


            PaymentDto payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.Timeout);
            Check.That(payment.Approved.Value).IsFalse();
            Check.That(payment.AcquiringBankPaymentId).IsNull();
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