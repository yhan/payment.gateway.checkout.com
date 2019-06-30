using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

        public Task<ICollection<GatewayPaymentId>> AllGatewayPaymentsIds()
        {
            return Task.FromResult(_map.Values);
        }

        public Task<ICollection<AcquiringBankPaymentId>> AllAcquiringBankPaymentsIds()
        {
            return Task.FromResult(_map.Keys);
        }
    }
}