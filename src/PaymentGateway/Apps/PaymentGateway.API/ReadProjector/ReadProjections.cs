using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using PaymentGateway.Infrastructure.ReadProjector;

namespace PaymentGateway.API.ReadProjector
{
    public class ReadProjections : IHostedService
    {
        private readonly PaymentReadProjector _paymentReadProjector;

        public ReadProjections(IPublishEvents bus, IPaymentDetailsRepository paymentDetailsRepository)
        {
            _paymentReadProjector = new PaymentReadProjector(bus, paymentDetailsRepository);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _paymentReadProjector.SubscribeToEventsForUpdatingReadModel();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _paymentReadProjector.UnsubscribeFromEventsForUpdatingReadModel();
            return Task.CompletedTask;
        }
    }
}
