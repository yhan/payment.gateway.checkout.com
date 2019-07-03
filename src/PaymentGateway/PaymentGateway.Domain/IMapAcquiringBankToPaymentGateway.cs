using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    /// <summary>
    /// Remember and Map acquiring bank's payment id to Gateway one.
    /// </summary>
    public interface IMapAcquiringBankToPaymentGateway : IKnowAllPaymentsIds
    {
        /// <summary>
        /// Map acquiring bank's payment id to our Gateway one
        /// </summary>
        /// <param name="paymentAcquiringBankId">Acquiring bank's payment id</param>
        /// <returns>Gateway payment id</returns>
        GatewayPaymentId GetPaymentGatewayId(AcquiringBankPaymentId paymentAcquiringBankId);

        /// <summary>
        /// Memorize a mapping: bank's payment id to our Gateway one
        /// </summary>
        void RememberMapping(AcquiringBankPaymentId acquiringBankPaymentId, GatewayPaymentId gatewayPaymentId);
    }

    /// <summary>
    /// Represent something who is aware of all payment ids (acquiring bank or gateway side)
    /// </summary>
    public interface IKnowAllPaymentsIds
    {
        Task<ICollection<GatewayPaymentId>> AllGatewayPaymentsIds();

        Task<ICollection<AcquiringBankPaymentId>> AllAcquiringBankPaymentsIds();
    }
}