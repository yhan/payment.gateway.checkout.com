using System;
using System.Threading.Tasks;

namespace AcquiringBanks.Stub
{
    public class BNP : IAmAcquiringBank
    {
        private readonly IGenerateBankPaymentId _bankPaymentIdGenerator;
        private readonly IGenerateAcquiringBankPaymentStatus _paymentStatusGenerator;
        private readonly IConnectToAcquiringBanks _connectionBehavior;

        public BNP(IGenerateBankPaymentId bankPaymentIdGenerator,
            IGenerateAcquiringBankPaymentStatus paymentStatusGenerator, IConnectToAcquiringBanks connectionBehavior)
        {
            _bankPaymentIdGenerator = bankPaymentIdGenerator;
            _paymentStatusGenerator = paymentStatusGenerator;
            _connectionBehavior = connectionBehavior;
        }

        public async Task<BNPResponse> RespondToPayment(BNPPaymentRequest request)
        {
            var bankPaymentId = _bankPaymentIdGenerator.Generate();

            var response = new BNPResponse(bankPaymentId, request.GatewayPaymentId, _paymentStatusGenerator.GeneratePaymentStatus());

            return await Task.FromResult(response);
        }

        public async Task<bool> Connect()
        {
            return await _connectionBehavior.Connect();
        }
    }

    public class BNPPaymentRequest
    {
        #region in simulation these properties won't be used

        public Guid GatewayPaymentId { get; }
        public double Amount { get; }
        public string Currency { get; }
        public string CardCvv { get; }
        public string CardExpiry { get; }
        public string CardNumber { get; }

        #endregion

        public BNPPaymentRequest(Guid gatewayPaymentId, double amount, string currency, string cardCvv, string cardExpiry, string cardNumber)
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