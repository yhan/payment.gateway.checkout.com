using System;

namespace PaymentGateway.Infrastructure
{
    public class SimulateGatewayException : IThrowsException
    {
        public void Throws()
        {
            throw new FakeException();
        }

        private class FakeException : Exception { }
    }

    public interface IThrowsException
    {
        void Throws();
    }

    public class NullThrows: IThrowsException
    {
        public void Throws()
        {
            
        }
    }
}