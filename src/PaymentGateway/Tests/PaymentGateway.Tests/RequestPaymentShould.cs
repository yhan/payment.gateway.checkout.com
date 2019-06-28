using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.API.WriteAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using SimpleCQRS;

namespace PaymentGateway.Tests
{
    public class PaymentCQRS
    {
        internal static (PaymentRequestsController, IProvidePaymentIdsMapping, IProcessPayment, AcquiringBankFacade ) Build()
        {
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(new FakeBus()));
            var controller = new PaymentRequestsController(eventSourcedRepository);

            var paymentIdsMapping = new InMemoryPaymentIdsMapping();


            var acquiringBank = new AcquiringBankFacade();
            var mediator = new AcquiringBanksMediator(acquiringBank, eventSourcedRepository);

            return (controller, paymentIdsMapping, mediator, acquiringBank);
        }
    }

    [TestFixture]
    public class RequestPaymentShould
    {
        private static PaymentRequest BuildPaymentRequest(Guid requestId)
        {
            return new PaymentRequest(requestId, "John Smith", "4524 4587 5698 1200", "05/19", new Money("EUR", 42.66),
                "321");
        }

        private static void CheckThatPaymentResourceIsCorrectlyCreated(IActionResult response, Guid paymentId,
            Guid requestId)
        {
            Check.That(response).IsInstanceOf<CreatedAtActionResult>();
            var created = ((CreatedAtActionResult) response).Value;
            Check.That(created).IsInstanceOf<PaymentDto>();

            var payment = (PaymentDto) created;

            Check.That(payment.GateWayPaymentId).IsEqualTo(paymentId);
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.Status).IsEqualTo(PaymentStatus.Pending);
        }

        [Test]
        public async Task Create_payment_When_handling_PaymentRequest()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = BuildPaymentRequest(requestId);

            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var (controller, _, paymentProcessor, _) = PaymentCQRS.Build();

            var response = await controller.ProceedPaymentRequest(paymentRequest, guidGenerator, new InMemoryPaymentIdsMapping(), paymentProcessor);

            CheckThatPaymentResourceIsCorrectlyCreated(response, gatewayPaymentId, requestId);
        }

        [Test]
        public async Task Not_handle_a_PaymentRequest_more_than_once()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = BuildPaymentRequest(requestId);

            var (controller, paymentIdsMapping, paymentProcessor, _) = PaymentCQRS.Build();
            
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);
            await controller.ProceedPaymentRequest(paymentRequest, guidGenerator, paymentIdsMapping, paymentProcessor);

            var actionResult = await controller.ProceedPaymentRequest(paymentRequest, guidGenerator, paymentIdsMapping, paymentProcessor);

            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult) actionResult;
            var failDetail = (ProblemDetails) badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Identical payment request will not be handled more than once");
        }

        [Test]
        [Repeat(20)]
        public async Task Return_payment_success_When_AcquiringBank_accepts_payment()
        {
            var requestId = Guid.NewGuid();
            var paymentRequest = BuildPaymentRequest(requestId);
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);

            var (controller, paymentIdsMapping, paymentProcessor, acquiringBank) = PaymentCQRS.Build();
            await controller.ProceedPaymentRequest(paymentRequest, guidGenerator, paymentIdsMapping, paymentProcessor);

            await acquiringBank.WaitForBankResponse();

            var payment = (await controller.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);
            Check.That(payment.Id).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.Success);
            Check.That(payment.Version).IsEqualTo(1);
        }
    }
}