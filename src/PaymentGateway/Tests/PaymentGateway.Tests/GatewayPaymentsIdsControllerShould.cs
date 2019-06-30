using System;
using System.Threading.Tasks;
using AcquiringBanks.API;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class GatewayPaymentsIdsControllerShould
    {
        [Test]
        public async Task Return_all_payments_s_GatewayId()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = Utils.BuildPaymentRequest(requestId);

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid gatewayPaymentIdGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior());

            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, gatewayPaymentIdGenerator, new InMemoryPaymentIdsMapping(), cqrs.PaymentProcessor);
            
            var gatewayPaymentsIds = await cqrs.GatewayPaymentsIdsController.Get();
            Check.That(gatewayPaymentsIds).ContainsExactly(gatewayPaymentId);
        }
    }


    [TestFixture]
    public class AcquiringBankPaymentsIdsControllerShould
    {
        [Test]
        public async Task Return_all_payments_s_AcquiringBankId()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = Utils.BuildPaymentRequest(requestId);

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid gatewayPaymentIdGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var acquiringBankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Accepted, new BankPaymentIdGeneratorForTests(acquiringBankPaymentId), new AlwaysSuccessBankConnectionBehavior());

            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, gatewayPaymentIdGenerator, new InMemoryPaymentIdsMapping(), cqrs.PaymentProcessor);
            
            var acquiringBankPaymentsIds = await cqrs.AcquiringBankPaymentsIdsController.Get();
            Check.That(acquiringBankPaymentsIds).ContainsExactly(acquiringBankPaymentId);
        }
    }
}