using System;
using System.Collections.Concurrent;
using System.Data;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentIdsMemory : IMapAcquiringBankToPaymentGateway
    {
        private readonly ConcurrentDictionary<AcquiringBankPaymentId, GatewayPaymentId> _map = new ConcurrentDictionary<AcquiringBankPaymentId, GatewayPaymentId>();
        
        public GatewayPaymentId GetPaymentGatewayId(AcquiringBankPaymentId paymentAcquiringBankId)
        {
            return _map[paymentAcquiringBankId];
        }

        public void RememberMapping(AcquiringBankPaymentId acquiringBankPaymentId, GatewayPaymentId gatewayPaymentId)
        {
            if (!_map.TryAdd(acquiringBankPaymentId, gatewayPaymentId))
            {
                throw new ConstraintException(
                    $"Bank paymentId {acquiringBankPaymentId} already maps to Gateway Payment Id {_map[acquiringBankPaymentId]}");
            }
        }
    }
}