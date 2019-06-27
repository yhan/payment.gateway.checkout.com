using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NFluent;
using NUnit.Framework;
using PaymentGateway.API;
using PaymentGateway.API.WriteAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class PaymentControllerShould
    {
        [Test]
        public async Task Create_payment_When_handling_PaymentRequest()
        {
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(new FakeBus()));
            var controller = new PaymentRequestsController(eventSourcedRepository);
            var requestId = Guid.NewGuid();
            var paymentRequest = BuildPaymentRequest(requestId);
            
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);
            IActionResult response = await controller.ProceedPaymentRequest(paymentRequest, guidGenerator, new InMemoryPaymentIdsMapping(), new AcquiringBanksMediator(new AcquiringBankFacade(), eventSourcedRepository));
            CheckThatPaymentResourceIsCorrectlyCreated(response, gatewayPaymentId, requestId);
        }

        private static PaymentRequest BuildPaymentRequest(Guid requestId)
        {
            return new PaymentRequest(requestId, "John Smith", "4524 4587 5698 1200", "05/19", new Money("EUR", 42.66), "321");
        }

        [Test]
        public async Task Not_handle_a_PaymentRequest_more_than_once()
        {
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(new FakeBus()));
            var controller = new PaymentRequestsController(eventSourcedRepository);
            var requestId = Guid.NewGuid();
            var paymentRequest = BuildPaymentRequest(requestId);;
            
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);
            var inMemoryPaymentIdsMapping = new InMemoryPaymentIdsMapping();
            await controller.ProceedPaymentRequest(paymentRequest, guidGenerator, inMemoryPaymentIdsMapping, new AcquiringBanksMediator(new AcquiringBankFacade(), eventSourcedRepository));

            var actionResult = await controller.ProceedPaymentRequest(paymentRequest, guidGenerator, inMemoryPaymentIdsMapping, new AcquiringBanksMediator(new AcquiringBankFacade(), eventSourcedRepository));
            Check.That(actionResult).IsInstanceOf<BadRequestObjectResult>();
            var badRequest = (BadRequestObjectResult) actionResult;
            var failDetail = (ProblemDetails)badRequest.Value;
            Check.That(failDetail.Detail).IsEqualTo("Identical payment request will not be handled more than once");

        }

        [Test]
        public async Task Return_payment_success_When_AcquiringBank_accepts_payment()
        {
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(new FakeBus()));
            var controller = new PaymentRequestsController(eventSourcedRepository);
            var requestId = Guid.NewGuid();
            var paymentRequest = BuildPaymentRequest(requestId);;
            
            var gatewayPaymentId = Guid.NewGuid();
            IGenerateGuid guidGenerator = new GuidGeneratorForTesting(gatewayPaymentId);
            var inMemoryPaymentIdsMapping = new InMemoryPaymentIdsMapping();
            await controller.ProceedPaymentRequest(paymentRequest, guidGenerator, inMemoryPaymentIdsMapping, new AcquiringBanksMediator(new AcquiringBankFacade(), eventSourcedRepository));

            var payment = (await controller.GetPaymentInfo(gatewayPaymentId)).Value;
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.GatewayPaymentId).IsEqualTo(gatewayPaymentId);
            Check.That(payment.Id).IsEqualTo(gatewayPaymentId);

            Check.That(payment.Status).IsEqualTo(PaymentStatus.Success);
        }

        private static void CheckThatPaymentResourceIsCorrectlyCreated(IActionResult response, Guid paymentId, Guid requestId)
        {
            Check.That(response).IsInstanceOf<CreatedAtActionResult>();
            var created = ((CreatedAtActionResult) response).Value;
            Check.That(created).IsInstanceOf<PaymentDto>();

            var payment = (PaymentDto) created;

            Check.That(payment.GateWayPaymentId).IsEqualTo(paymentId);
            Check.That(payment.RequestId).IsEqualTo(requestId);
            Check.That(payment.Status).IsEqualTo(PaymentStatus.Pending);
        }
    }
}