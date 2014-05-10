using System.Security.Cryptography;
using System.Threading;

namespace Stomp2
{
    public class StompClient : IStompClient
    {
        private readonly StompTransport _transport;

        private readonly IDestinationStorage _destinationStorage;

        public StompClient(string address, int port)
        {
            _transport = new StompTransport(address, port);
            _destinationStorage = new DestinationStorage(_transport);
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
            return _destinationStorage.Get(destination);
        }

    }
}