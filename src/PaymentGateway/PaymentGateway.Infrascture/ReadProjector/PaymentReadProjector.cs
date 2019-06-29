using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Domain.Events;
using SimpleCQRS;

namespace PaymentGateway.Infrastructure.ReadProjector
{
    public class PaymentReadProjector
    {
        private readonly IPublishEvents _bus;
        private readonly IPaymentDetailsRepository _paymentDetailsRepository;

        public PaymentReadProjector(IPublishEvents bus, IPaymentDetailsRepository paymentDetailsRepository)
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

        public void UnsubscribeFromEventsForUpdatingReadModel()
        {
            _bus.UnRegisterHandlers();
        }

        private async Task Handle(PaymentFaulted faulted)
        {
            await _paymentDetailsRepository.Update(faulted.GatewayPaymentId, faulted.BankPaymentId, PaymentStatus.FaultedOnGateway);
        }

        private async Task Handle(PaymentRejectedByBank rejectedByBank)
        {
            await _paymentDetailsRepository.Update(rejectedByBank.GatewayPaymentId, rejectedByBank.BankPaymentId, PaymentStatus.RejectedByBank);
        }

        private async Task Handle(PaymentSucceeded succeeded)
        {
            await _paymentDetailsRepository.Update(succeeded.GatewayPaymentId, succeeded.BankPaymentId, PaymentStatus.Success);
        }

        private async Task Handle(PaymentRequested requested)
        {
            await _paymentDetailsRepository.Create(requested.GatewayPaymentId, requested.CreditCard);
        }
    }
}