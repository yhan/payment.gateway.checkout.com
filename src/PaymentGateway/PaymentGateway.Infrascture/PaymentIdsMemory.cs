using System;
using System.Collections.Concurrent;
using System.Data;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentIdsMemory : IMapAcquiringBankToPaymentGateway
    {
        private readonly ConcurrentDictionary<Guid, Guid> _map = new ConcurrentDictionary<Guid, Guid>();
        
        public Guid GetPaymentGatewayId(Guid paymentAcquiringBankId)
        {
            return _map[paymentAcquiringBankId];
        }

        public void RememberMapping(PaymentIds paymentIds)
        {
            var paymentAcquiringBankId = paymentIds.BankPaymentId;
            if (!_map.TryAdd(paymentAcquiringBankId, paymentIds.GatewayPaymentId))
            {
                throw new ConstraintException(
                    $"Bank paymentId {paymentAcquiringBankId} already maps to Gateway Payment Id {_map[paymentAcquiringBankId]}");
            }
        }
    }
}