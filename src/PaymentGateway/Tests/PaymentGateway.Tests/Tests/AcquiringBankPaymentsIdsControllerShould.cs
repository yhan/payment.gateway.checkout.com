using System;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using PaymentGateway;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class AcquiringBankPaymentsIdsControllerShould
    {
        [Test]
        public async Task Return_all_payments_s_AcquiringBankId()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = TestsUtils.BuildPaymentRequest(requestId, MerchantsRepository.Amazon);

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid gatewayPaymentIdGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var acquiringBankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(acquiringBankPaymentId), new AlwaysSuccessBankConnectionBehavior(), new DelayProviderForTesting(TimeSpan.FromMilliseconds(1)), PaymentCQRS.TimeoutProviderForBankResponseWaiting(TimeSpan.FromMilliseconds(200)), Substitute.For<IKnowBufferAndReprocessPaymentRequest>());

            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, gatewayPaymentIdGenerator, new PaymentRequestsMemory(), cqrs.PaymentProcessor);
            
            var acquiringBankPaymentsIds = await cqrs.AcquiringBankPaymentsIdsController.Get();
            Check.That(acquiringBankPaymentsIds).ContainsExactly(acquiringBankPaymentId);
        }
    }
}