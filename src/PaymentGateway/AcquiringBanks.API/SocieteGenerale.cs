using System;
using System.Threading.Tasks;

namespace AcquiringBanks.Stub
{
    public class SocieteGenerale : IAmAcquiringBank
    {
        private readonly IGenerateBankPaymentId _bankPaymentIdGenerator;
        private readonly IRandomnizeAcquiringBankPaymentStatus _paymentStatusGenerator;
        private readonly IConnectToAcquiringBanks _connectionBehavior;

        public SocieteGenerale(IGenerateBankPaymentId bankPaymentIdGenerator,
            IRandomnizeAcquiringBankPaymentStatus paymentStatusGenerator, IConnectToAcquiringBanks connectionBehavior)
        {
            _bankPaymentIdGenerator = bankPaymentIdGenerator;
            _paymentStatusGenerator = paymentStatusGenerator;
            _connectionBehavior = connectionBehavior;
        }

        /// <summary>
        /// Simulate network call 
        /// </summary>
        public async Task<SocieteGeneraleResponse> RespondToPayment(SocieteGeneralePaymentRequest request)
        {
            var bankPaymentId = _bankPaymentIdGenerator.Generate();

            var response = new SocieteGeneraleResponse(bankPaymentId, request.GatewayPaymentId, _paymentStatusGenerator.GeneratePaymentStatus());

            return await Task.FromResult(response);
        }

        public async Task<bool> Connect()
        {
            return await _connectionBehavior.Connect();
        }
    }

    public class SocieteGeneralePaymentRequest
    {
        #region in simulation these properties won't be used

        public Guid GatewayPaymentId { get; }
        public double Amount { get; }
        public string Currency { get; }
        public string CardCvv { get; }
        public string CardExpiry { get; }
        public string CardNumber { get; }
        

        #endregion
        public SocieteGeneralePaymentRequest(Guid gatewayPaymentId, double amount, string currency, string cardCvv, string cardExpiry, string cardNumber)
        {
            GatewayPaymentId = gatewayPaymentId;
            Amount = amount;
            Currency = currency;
            CardCvv = cardCvv;
            CardExpiry = cardExpiry;
            CardNumber = cardNumber;
        }
    }

}