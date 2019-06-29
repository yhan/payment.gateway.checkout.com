using System;
using PaymentGateway.Domain;
using PaymentGateway.Domain.Events;
using SimpleCQRS;

namespace PaymentGateway.Infrastructure.ReadProjector
{
    public class PaymentReadProjector
    {
        private readonly InMemoryBus _bus;
        private readonly IPaymentDetailsRepository _paymentDetailsRepository;

        public PaymentReadProjector(InMemoryBus bus, IPaymentDetailsRepository paymentDetailsRepository)
        {
            _bus = bus;
            _paymentDetailsRepository = paymentDetailsRepository;
        }

        public void SubscribeToEventsForUpdatingReadModel()
        {
            _bus.RegisterHandler<PaymentRequested>(Handle);
            _bus.RegisterHandler<PaymentSucceeded>(Handle);
            _bus.RegisterHandler<PaymentRejectedByBank>(Handle);
            _bus.RegisterHandler<PaymentFaulted>(Handle);
        }

        private void Handle(PaymentFaulted faulted)
        {
            _paymentDetailsRepository.Update(faulted.GatewayPaymentId, faulted.BankPaymentId, PaymentStatus.FaultedOnGateway);
        }

        private void Handle(PaymentRejectedByBank rejectedByBank)
        {
            _paymentDetailsRepository.Update(rejectedByBank.GatewayPaymentId, rejectedByBank.BankPaymentId, PaymentStatus.RejectedByBank);
        }

        private void Handle(PaymentSucceeded succeeded)
        {
            _paymentDetailsRepository.Update(succeeded.GatewayPaymentId, succeeded.BankPaymentId, PaymentStatus.Success);
        }

        private void Handle(PaymentRequested requested)
        {
            _paymentDetailsRepository.Create(requested.GatewayPaymentId, requested.CreditCard);
        }
    }
}