using System;
using System.Threading.Tasks;
using AcquiringBanks.API;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NUnit.Framework;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class ReadPaymentShould
    {
        [Test]
        public async Task Return_NotFound_When_Payment_does_not_exist()
        {
            var cqrs = PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Rejected);
            var nonExistingPaymentId = Guid.NewGuid();
            var actionResult = await cqrs.ReadController.GetPaymentInfo(nonExistingPaymentId);

            Check.That(actionResult.Result).IsInstanceOf<NotFoundResult>();
            Check.That(actionResult.Value).IsNull();
        }
    }
}