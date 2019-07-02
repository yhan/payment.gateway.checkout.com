using System;
using AcquiringBanks.API;

namespace PaymentGateway.Infrastructure
{
    //public interface ITalkToAcquiringBank
    //{
    //    Task<IBankResponse> Pay(PayingAttempt paymentAttempt);
    //}

    ///// <summary>
    ///// Adapter for AcquiringBank API and PaymentGateway API
    ///// </summary>
    //public class AcquiringBankFacade : ITalkToAcquiringBank
    //{
    //    private readonly IAmAcquiringBank _bank;
    //    private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;
    //    private readonly IProvideBankAdapters _bankAdaptersProvider;

    //    public AcquiringBankFacade(IAmAcquiringBank bank, IMapAcquiringBankToPaymentGateway paymentIdsMapper, IProvideBankAdapters bankAdaptersProvider)
    //    {
    //        _bank = bank;
    //        _paymentIdsMapper = paymentIdsMapper;
    //        _bankAdaptersProvider = bankAdaptersProvider;
    //    }

    //    public async Task<IBankResponse> Pay(PayingAttempt paymentAttempt)
    //    {
    //        // Connection to bank
    //        var policy = Policy.Handle<FailedConnectionToBankException>()
    //                           .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(Math.Pow(2, retry)));

    //        var policyResult = await policy.ExecuteAndCaptureAsync(async () => await _bank.Connect());
    //        if (!policyResult.Result)
    //        {
    //            return new BankDoesNotRespond(paymentAttempt.GatewayPaymentId);
    //        }

    //        // get bank adapter
    //        IAdaptToBank adapter = _bankAdaptersProvider.FindBankAdapter(paymentAttempt.MerchantId);
            


    //        ////Adapt PaymentGateway to AcquiringBank
    //        //string bankResponseJson = await _bank.RespondsTo(paymentAttempt);
            
    //        //Adapt AcquiringBank back to PaymentGateway
    //        BankResponse bankResponse = await adapter.RespondToPaymentAttempt(paymentAttempt);

    //        _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(bankResponse.BankPaymentId), new GatewayPaymentId(bankResponse.GatewayPaymentId));

    //        return bankResponse;
    //    }
    //}

    public class BankDoesNotRespond : IBankResponse
    {
        public Guid GatewayPaymentId { get; }

        public BankDoesNotRespond(Guid gatewayPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
        }

        public bool BankContactable()
        {
            return false;
        }
    }
}