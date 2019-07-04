using System;
using System.Net;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using PaymentGateway.API;
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

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)));

            var response = await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, new PaymentRequestsMemory(), cqrs.PaymentProcessor);

            CheckThatPaymentResourceIsCorrectlyCreated(response, gatewayPaymentId, requestId);
        }

        [Test]
        public async Task Not_handle_a_PaymentRequest_more_than_once()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)));

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
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)));
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

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)), new SimulateGatewayException());
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
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new FailTwiceBankThenSuccessConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)));
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(expectedPaymentStatusReturnedByGateway);
            Check.That(payment.Approved).IsEqualTo(payment.Status == PaymentGateway.Domain.PaymentStatus.Success);
            Check.That(payment.AcquiringBankPaymentId).IsEqualTo(bankPaymentId);
        }

        [TestCase(BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.ReceivedDuplicatedBankPaymentIdFailure)]
        [TestCase(BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.ReceivedDuplicatedBankPaymentIdFailure)]
        public async Task Return_InternalServerEror_When_AcquiringBank_sent_duplicated_PaymentId(BankPaymentStatus bankPaymentStatus, PaymentStatus expectedPaymentStatusReturnedByGateway)
        {
            var requestId = Guid.Parse("9cb25b95-45ba-4100-a180-deb13259d0e1");
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);
            var secondRequestId = Guid.Parse("9c940897-b1c4-4598-96a7-82502ca51845");
            var secondPaymentRequest = TestsUtils.BuildPaymentRequest(secondRequestId, MerchantsRepository.Apple);

            var gatewayPaymentId = Guid.Parse("002ee45f-fdfb-4666-b504-70aa26ecf646");
            var secondGatewayPaymentId = Guid.Parse("4cc3dd04-b3d8-4be4-8c68-e744c387fb6c");
            IGenerateGuid gatewayPaymentIdGenerator = Substitute.For<IGenerateGuid>();
            gatewayPaymentIdGenerator.Generate().Returns(gatewayPaymentId, secondGatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)));
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, gatewayPaymentIdGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);
            
            await cqrs.RequestsController.ProceedPaymentRequest(secondPaymentRequest, gatewayPaymentIdGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);
            
            var secondFailedPayment = (await cqrs.PaymentReadController.GetPaymentInfo(secondGatewayPaymentId)).Value;
            Check.That(secondFailedPayment.RequestId).IsEqualTo(secondRequestId);
            Check.That(secondFailedPayment.GatewayPaymentId).IsEqualTo(secondGatewayPaymentId);

            Check.That(secondFailedPayment.Status).IsEqualTo(expectedPaymentStatusReturnedByGateway);
            Check.That(secondFailedPayment.Approved).IsEqualTo(secondFailedPayment.Status == PaymentGateway.Domain.PaymentStatus.Success);
            Check.That(secondFailedPayment.AcquiringBankPaymentId).IsNull();
        }
        
        //
        [TestCase(BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.ReceivedDuplicatedBankPaymentIdFailure)]
        [TestCase(BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.ReceivedDuplicatedBankPaymentIdFailure)]
        public async Task Return_InternalServerEror_When_AcquiringBank_sent_duplicated_PaymentId_Using_StupidBank(BankPaymentStatus bankPaymentStatus, PaymentStatus expectedPaymentStatusReturnedByGateway)
        {
            var requestId = Guid.Parse("9cb25b95-45ba-4100-a180-deb13259d0e1");
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.FailFromThe2ndPaymentMerchant);
            var secondRequestId = Guid.Parse("9c940897-b1c4-4598-96a7-82502ca51845");
            var secondPaymentRequest = TestsUtils.BuildPaymentRequest(secondRequestId, MerchantsRepository.FailFromThe2ndPaymentMerchant);

            var gatewayPaymentId = Guid.Parse("002ee45f-fdfb-4666-b504-70aa26ecf646");
            var secondGatewayPaymentId = Guid.Parse("4cc3dd04-b3d8-4be4-8c68-e744c387fb6c");
            IGenerateGuid gatewayPaymentIdGenerator = Substitute.For<IGenerateGuid>();
            gatewayPaymentIdGenerator.Generate().Returns(gatewayPaymentId, secondGatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, new DefaultBankPaymentIdGenerator(), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)));
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, gatewayPaymentIdGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);
            
            await cqrs.RequestsController.ProceedPaymentRequest(secondPaymentRequest, gatewayPaymentIdGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);
            
            var secondFailedPayment = (await cqrs.PaymentReadController.GetPaymentInfo(secondGatewayPaymentId)).Value;
            Check.That(secondFailedPayment.RequestId).IsEqualTo(secondRequestId);
            Check.That(secondFailedPayment.GatewayPaymentId).IsEqualTo(secondGatewayPaymentId);

            Check.That(secondFailedPayment.Status).IsEqualTo(expectedPaymentStatusReturnedByGateway);
            Check.That(secondFailedPayment.Approved).IsEqualTo(secondFailedPayment.Status == PaymentGateway.Domain.PaymentStatus.Success);
            Check.That(secondFailedPayment.AcquiringBankPaymentId).IsNull();
        }
        [Test]
        public async Task Return_BankUnavailable_When_connection_to_bank_is_broken_or_bank_API_is_down()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Apple);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var delayProviderForTesting = new DelayProviderForTesting(TimeSpan.FromMilliseconds(1));
            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysFailBankConnectionBehavior(), delayProviderForTesting, PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)), new NullThrows());
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

            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)), new NullThrows());
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
            
            var timeoutTolerance = TimeSpan.FromMilliseconds(20);
            var delayBiggerThanTolerance = timeoutTolerance * 2;
            var cqrs = await PaymentCQRS.Build( BankPaymentStatus.Accepted, 
                                                new BankPaymentIdGeneratorForTests(bankPaymentId), 
                                                new AlwaysSuccessBankConnectionBehavior(), 
                                                new DelayProviderForTesting(delayBiggerThanTolerance), 
                                                PaymentCQRS.TimeoutProviderForBankResponseWaiting(timeoutTolerance));

            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);


            PaymentDto payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.Timeout);
            Check.That(payment.Approved.Value).IsFalse();
            Check.That(payment.AcquiringBankPaymentId).IsNull();
        }


        [TestCase(BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.Success)]
        [TestCase(BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.RejectedByBank)]
        public async Task Return_proper_payment_status_When_call_bank_api_timeout_twice_then_succeed_AND_AcquiringBank_accepts_or_reject_payment(BankPaymentStatus bankPaymentStatus, PaymentStatus expectedPaymentStatusReturnedByGateway)
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");

            var smallLatency = TimeSpan.FromMilliseconds(1);
            TimeSpan bigLatency = smallLatency * 3;

            var bigLatencyTwiceThenSmallLatencyTimeoutProvider = NSubstitute.Substitute.For<IProvideTimeout>();
            bigLatencyTwiceThenSmallLatencyTimeoutProvider.GetTimeout().Returns(x => bigLatency, x => bigLatency, x => smallLatency);
            
            var cqrs = await PaymentCQRS.Build(bankPaymentStatus, 
                new BankPaymentIdGeneratorForTests(bankPaymentId), 
                new AlwaysSuccessBankConnectionBehavior(), 
                new DelayProviderForTesting(smallLatency), 
                bigLatencyTwiceThenSmallLatencyTimeoutProvider);
            
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequestsMemory, cqrs.PaymentProcessor);
            
            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(expectedPaymentStatusReturnedByGateway);
            Check.That(payment.Approved).IsEqualTo(payment.Status == PaymentGateway.Domain.PaymentStatus.Success);
            Check.That(payment.AcquiringBankPaymentId).IsEqualTo(bankPaymentId);
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