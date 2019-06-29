using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.Domain;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class ReadPaymentShould
    {
        [Test]
        public async Task Return_NotFound_When_Payment_does_not_exist()
        {
            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Rejected, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")));
            var nonExistingPaymentId = Guid.NewGuid();
            var actionResult = await cqrs.PaymentReadController.GetPaymentInfo(nonExistingPaymentId);

            Check.That(actionResult.Result).IsInstanceOf<NotFoundObjectResult>();
            Check.That(actionResult.Value).IsNull();
        }

        [Repeat(10)]
        [TestCase(AcquiringBanks.API.BankPaymentStatus.Rejected, PaymentGateway.Domain.PaymentStatus.RejectedByBank)]
        [TestCase(AcquiringBanks.API.BankPaymentStatus.Accepted, PaymentGateway.Domain.PaymentStatus.Success)]
        public async Task Can_retrieve_payment_details_using_BankPaymentId(AcquiringBanks.API.BankPaymentStatus paymentBankStatus, PaymentGateway.Domain.PaymentStatus expectedStatusInPaymentDetails
        )
        {
            var requestId = Guid.NewGuid();
            var paymentRequest =  new PaymentRequest(requestId, "John Smith", "4524 4587 5698 1200", "05/19", new Money("EUR", 42.66),
                "321");

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var bankPaymentId = Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0");
            var cqrs = await PaymentCQRS.Build(paymentBankStatus, new BankPaymentIdGeneratorForTests(bankPaymentId));
            await cqrs.RequestsController.ProceedPaymentRequest(paymentRequest, guidGenerator, cqrs.PaymentIdsMapping, cqrs.PaymentProcessor);


            var payment = (await cqrs.PaymentReadController.GetPaymentInfo(gatewayPaymentId)).Value;
            var paymentDetails = (await cqrs.PaymentDetailsReadController.GetPaymentDetails(payment.AcquiringBankPaymentId)).Value;

            // The response should include a masked card number and card details along with a
            // status code which indicates the result of the payment.
            Check.That(paymentDetails.CreditCardNumber).IsEqualTo("4524 XXXX XXXX XXXX");
            Check.That(paymentDetails.CreditCardHolderName).IsEqualTo("John Smith");
            Check.That(paymentDetails.CreditCardExpiry).IsEqualTo("05/19");
            Check.That(paymentDetails.CreditCardCvv).IsEqualTo("321");
            Check.That(paymentDetails.Status).IsEqualTo(expectedStatusInPaymentDetails);
            Check.That(paymentDetails.AcquiringBankPaymentId).IsEqualTo((bankPaymentId));
        }


        [Test]
        public async Task Return_NotFound_When_PaymentDetails_does_not_exist()
        {
            var cqrs = await PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Rejected, new BankPaymentIdGeneratorForTests(Guid.Parse("3ec8c76c-7dc2-4769-96f8-7e0649ecdfc0")));
            var nonExistingBankPaymentId = Guid.NewGuid();
            var actionResult = await cqrs.PaymentDetailsReadController.GetPaymentDetails(nonExistingBankPaymentId);

            Check.That(actionResult.Result).IsInstanceOf<NotFoundObjectResult>();
            Check.That(actionResult.Value).IsNull();
        }

    }
}