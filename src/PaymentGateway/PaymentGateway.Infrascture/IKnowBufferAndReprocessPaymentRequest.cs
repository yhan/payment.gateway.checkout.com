using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using Polly;

namespace PaymentGateway.Infrastructure
{
    public interface IKnowBufferAndReprocessPaymentRequest
    {
        void Buffer(IAdaptToBank bankAdapter, PayingAttempt payingAttempt, Payment payment);
        Task ProcessBufferedPaymentRequest();
    }

    public class PaymentRequestsLaterHandler : IKnowBufferAndReprocessPaymentRequest
    {
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly ILogger<PaymentProcessor> _logger;
        private readonly IProvideTimeout _timeoutProviderForBankResponseWaiting;
        private readonly ILogger<PaymentRequestsLaterHandler> _bankResponseProcessingLogger;
        private readonly IThrowsException _gatewayExceptionSimulator;
        private readonly ConcurrentQueue<PaymentRequestBuffer> _buffer = new ConcurrentQueue<PaymentRequestBuffer>();

        public PaymentRequestsLaterHandler(IEventSourcedRepository<Payment> paymentsRepository,
            ILogger<PaymentProcessor> logger,
            IProvideTimeout timeoutProviderForBankResponseWaiting,

            ILogger<PaymentRequestsLaterHandler> bankResponseProcessingLogger,
            IThrowsException gatewayExceptionSimulator = null)
        {
            _paymentsRepository = paymentsRepository;
            _logger = logger;
            _timeoutProviderForBankResponseWaiting = timeoutProviderForBankResponseWaiting;
            _bankResponseProcessingLogger = bankResponseProcessingLogger;
            _gatewayExceptionSimulator = gatewayExceptionSimulator;
        }

        public void Buffer(IAdaptToBank bankAdapter, PayingAttempt payingAttempt, Payment payment)
        {
            _buffer.Enqueue(new PaymentRequestBuffer(bankAdapter, payingAttempt, payment));
        }

        public async Task ProcessBufferedPaymentRequest()
        {
            while (!_buffer.IsEmpty)
            {
                if (_buffer.TryDequeue(out var bufferItem))
                {
                    await UnitProcessingFailedBufferedPaymentRequest(bufferItem.BankAdapter, bufferItem.Payment, bufferItem.PayingAttempt);
                }
            }
        }

        private async Task UnitProcessingFailedBufferedPaymentRequest(IAdaptToBank bankAdapter, Payment payment, PayingAttempt payingAttempt)
        {
            IBankResponse bankResponse = new NullBankResponse();
            // Connection to bank
            var policy = Policy.Handle<TaskCanceledException>()
                .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(Math.Pow(2, retry)));

            var policyResult = await policy.ExecuteAndCaptureAsync(async () =>
            {
                using (var cts = new CancellationTokenSource())
                {
                    var timeout = _timeoutProviderForBankResponseWaiting.GetTimeout();
                    cts.CancelAfter(timeout);

                    bankResponse = await bankAdapter.RespondToPaymentAttempt(payingAttempt, cts.Token);
                }
            });

            if (policyResult.FinalException != null)
            {
                if (policyResult.FinalException is TaskCanceledException)
                {
                    _logger.LogError($"Payment gatewayId='{payingAttempt.GatewayPaymentId}' requestId='{payingAttempt.PaymentRequestId}' Timeout");

                    payment.Timeout();
                    await _paymentsRepository.Save(payment, payment.Version);

                }

                if (policyResult.FinalException is BankPaymentDuplicatedException paymentDuplicatedException)
                {
                    _logger.LogError(paymentDuplicatedException.Message);

                    payment.HandleBankPaymentIdDuplication();
                    await _paymentsRepository.Save(payment, payment.Version);

                }
            }

            var strategy = Build(bankResponse, _paymentsRepository);

            await strategy.Handle(_gatewayExceptionSimulator, bankResponse.GatewayPaymentId);
        }


        private IHandleBankResponseStrategy Build(IBankResponse bankResponse, IEventSourcedRepository<Payment> paymentsRepository)
        {
            switch (bankResponse)
            {
                case BankResponse response:
                    return new RespondedBankStrategy(response, paymentsRepository, _bankResponseProcessingLogger);

                case BankDoesNotRespond _:
                    return new NotRespondedBankStrategy(paymentsRepository);
            }

            throw new ArgumentException();
        }

    }

}