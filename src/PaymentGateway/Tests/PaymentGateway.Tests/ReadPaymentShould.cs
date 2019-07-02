using System;
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
    public class ReadPaymentShould
    {
        [Test]
        public async Task Return_NotFound_When_Payment_does_not_exist()
        {
            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Rejected, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior());
            var nonExistingPaymentId = Guid.NewGuid();
            var actionResult = await cqrs.PaymentReadController.GetPaymentInfo(nonExistingPaymentId);

            Check.That(actionResult.Result).IsInstanceOf<NotFoundObjectResult>();
            Check.That(actionResult.Value).IsNull();
        }

        [Repeat(10)]
        [TestCase(BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.RejectedByBank)]
        [TestCase(BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.Success)]
        public async Task Can_retrieve_payment_details_using_BankPaymentId(BankPaymentStatus paymentBankStatus, PaymentGateway.Domain.PaymentStatus expectedStatusInPaymentDetails
        )
        {
            var requestId = Guid.NewGuid();
            var paymentRequest =  new PaymentRequest(requestId, MerchantToBankAdapterMapper.Amazon,  new Money("EUR", 42.66), new PaymentGateway.Infrastructure.Card("4524 4587 5698 1200", "05/19", "321"));

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(paymentBankStatus, new BankPaymentIdGeneratorForTests(bankPaymentId), new AlwaysSuccessBankConnectionBehavior());
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentRequests, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;

            Check.That(payment.AcquiringBankPaymentId).HasAValue();
            var paymentDetails = (await cqrs.PaymentDetailsReadController.GetPaymentDetails(payment.AcquiringBankPaymentId.Value)).Value;

            // The response should include a masked card number and card details along with a
            // status code which indicates the result of the payment.
            Check.That(paymentDetails.Card.Number).IsEqualTo("4524 XXXX XXXX XXXX");
            Check.That(paymentDetails.Card.Expiry).IsEqualTo("05/19");
            Check.That(paymentDetails.Card.Cvv).IsEqualTo("321");
            Check.That(paymentDetails.Status).IsEqualTo(expectedStatusInPaymentDetails);
            Check.That(paymentDetails.Approved).IsEqualTo(expectedStatusInPaymentDetails == PaymentStatus.Success);
            Check.That(paymentDetails.AcquiringBankPaymentId).IsEqualTo(bankPaymentId);
        }


        [Test]
        public async Task Return_NotFound_When_PaymentDetails_does_not_exist()
        {
            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Rejected, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")), new AlwaysSuccessBankConnectionBehavior());
            var nonExistingBankPaymentId = Guid.NewGuid();
            var actionResult = await cqrs.PaymentDetailsReadController.GetPaymentDetails(nonExistingBankPaymentId);

            Check.That(actionResult.Result).IsInstanceOf<NotFoundObjectResult>();
            Check.That(actionResult.Value).IsNull();
        }
    }
}