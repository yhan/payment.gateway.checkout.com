﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;
using SimpleCQRS;

namespace PaymentGateway.API.WriteAPI
{
    [Route("api/PaymentRequests")]
    [ApiController]
    public class PaymentRequestsController : ControllerBase
    {
        private readonly IEventSourcedRepository<Payment> _repository;

        public PaymentRequestsController(IEventSourcedRepository<Payment> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> ProceedPaymentRequest([FromBody]PaymentRequest paymentRequest, 
            [FromServices]IGenerateGuid guidGenerator,
            [FromServices]IProvidePaymentIdsMapping paymentIdsMapping,
            [FromServices]IProcessPayment acquiringBank)
        {
            var gatewayPaymentId = guidGenerator.Generate();

            var handler = new PaymentRequestCommandHandler(_repository, paymentIdsMapping, acquiringBank);
            var commandResult = await handler.Handle(paymentRequest.AsCommand(gatewayPaymentId));
            switch (commandResult)
            {
                case SuccessCommandResult<Payment> success:
                    return CreatedAtAction(actionName: "GetPaymentInfo", routeValues: new {gateWayPaymentId = Guid.NewGuid()}, value: success.Entity.AsDto());
                case InvalidCommandResult invalid:
                    return ActionResultHelper.ToActionResult(invalid);
                default:
                    throw new NotSupportedException();
            }
        }

        [HttpGet("{gateWayPaymentId}", Name = nameof(GetPaymentInfo))]
        public async Task<ActionResult<Payment>> GetPaymentInfo([FromRoute]Guid gateWayPaymentId)
        {
            var payment = await _repository.GetById(gateWayPaymentId);

            return payment;
        }
        
    }
}