using Microsoft.Extensions.Options;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway
{
    public class RequestBankSynchronyMaster : IKnowSendRequestToBankSynchrony
    {
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;

        public RequestBankSynchronyMaster(IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public bool SendPaymentRequestAsynchronously()
        {
            return _optionsMonitor.CurrentValue.Executor == ExecutorType.API;
        }
    }
}