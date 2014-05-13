using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using StompSharp.Messages;
using StompSharp.Transport;

namespace StompSharp
{
    public class DestinationStorage : IDestinationStorage
    {
        private readonly StompTransport _transport;

        private readonly ConcurrentDictionary<string, RefCountStompDestination> _destinationsByName =
            new ConcurrentDictionary<string, RefCountStompDestination>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ConcurrentDictionary<int, Subject<IMessage>> _destinationsById =
            new ConcurrentDictionary<int, Subject<IMessage>>();

        private int _idSequence;

        public DestinationStorage(StompTransport transport)
        {
            _transport = transport;
            _transport.IncommingMessages.GetObservable("MESSAGE").Subscribe(OnMessage);
        }

        private void OnMessage(IMessage message)
        {
            var idHeader = message.Headers.FirstOrDefault(h => h.Key == "subscription");
            int id;

            if (idHeader == null || !int.TryParse(idHeader.Value.ToString(), out id))
            {
                return;
            }

            Subject<IMessage> subject;
            if (!_destinationsById.TryGetValue(id, out subject))
            {
                return;
            }

            subject.OnNext(message);
        }


        public IDestination Get(string destination)
        {
            var retVal = _destinationsByName.GetOrAdd(destination, CreateDestination);

            retVal.Increment();

            return retVal;
        }

        private RefCountStompDestination CreateDestination(string destination)
        {
            var subject = new Subject<IMessage>();
            int id = Interlocked.Increment(ref _idSequence);

            _destinationsById[id] = subject;

            var retVal =
                new RefCountStompDestination(
                    new StompDestination(_transport, destination, id, subject), OnDispose);


            return retVal;
        }

        private void OnDispose(RefCountStompDestination obj)
        {
            RefCountStompDestination value;
            Subject<IMessage> subject;

            if (_destinationsById.TryRemove(obj.Id, out subject))
            {
                subject.Dispose();
            }
            
            _destinationsByName.TryRemove(obj.Destination, out value);
        }
    }
}