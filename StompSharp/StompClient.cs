using System.Threading;

namespace Stomp2
{
    public class StompClient : IStompClient
    {
        private readonly StompTransport _transport;

        private int _destinationIds;

        public StompClient(string address, int port)
        {
            _transport = new StompTransport(address, port);
        }

        public void Dispose()
        {
            _transport.Dispose();
        }

        public ITransport Transport
        {
            get { return _transport; }
        }

        public IDestination GetDestination(string destination)
        {
            return new StompDestination(_transport, destination, Interlocked.Increment(ref _destinationIds));
        }

    }
}