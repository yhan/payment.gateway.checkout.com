using System;
using System.Collections.Concurrent;
using System.Data;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class BankToGatewayMapper : IMapAcquiringBankToPaymentGateway
    {
        private readonly ConcurrentDictionary<Guid, Guid> _map = new ConcurrentDictionary<Guid, Guid>();
        
        public Guid GetPaymentGatewayId(Guid gatewayPaymentId)
        {
            return _map[gatewayPaymentId];
        }

        public void RememberMapping(PaymentIds paymentIds)
        {
            var bankPaymentId = paymentIds.BankPaymentId;
            if (!_map.TryAdd(bankPaymentId, paymentIds.GatewayPaymentId))
            {
                throw new ConstraintException(
                    $"Bank paymentId {bankPaymentId} already maps to Gateway Payment Id {_map[bankPaymentId]}");
            }
        }
    }
}