using System;

namespace PaymentGateway.Infrastructure
{
    public class SimulateGatewayException
    {
        public void Throws()
        {
            throw new FakeException();
        }

        private class FakeException : Exception { }
    }
}