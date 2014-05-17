using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading;
using StompSharp.Messages;
using StompSharp.Transport;

namespace StompSharp
{
    internal class DestinationStorage : IDestinationStorage
    {
        private class DestinationKey
        {
            private readonly string _destination;
            private readonly object _subscriptionBehavior;

            public DestinationKey(string destination, object subscriptionBehavior)
            {
                _destination = destination;
                _subscriptionBehavior = subscriptionBehavior;
            }

            private bool Equals(DestinationKey other)
            {
                return string.Equals(_destination, other._destination) && _subscriptionBehavior.Equals(other._subscriptionBehavior);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((DestinationKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_destination.GetHashCode()*397) ^ _subscriptionBehavior.GetHashCode();
                }
            }
        }

        private readonly StompTransport _transport;

        private readonly ConcurrentDictionary<DestinationKey, object> _destinationsByName =
            new ConcurrentDictionary<DestinationKey, object>();

        private readonly ConcurrentDictionary<int, Subject<IMessage>> _destinationsById =
            new ConcurrentDictionary<int, Subject<IMessage>>();

        private int _idSequence;

        public DestinationStorage(StompTransport transport)
        {
            _transport = transport;
            _transport.IncommingMessages.GetObservable("MESSAGE").Subscribe(OnMessage);
        }

        public IDestination<TMessage> Get<TMessage>(string destination, ISubscriptionBehavior<TMessage> subscriptionBehavior) 
            where TMessage : IMessage
        {
            var retVal = _destinationsByName.GetOrAdd(new DestinationKey(destination, subscriptionBehavior), @neverMind => CreateDestination(destination, subscriptionBehavior));

            var castedRetVal = (RefCountStompDestination<TMessage>) retVal;

            castedRetVal.Increment();

            return castedRetVal;
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



        private RefCountStompDestination<TMessage> CreateDestination<TMessage>(string destination, ISubscriptionBehavior<TMessage> subscriptionBehavior) 
            where TMessage : IMessage
        {
            var destinationKey = new DestinationKey(destination, subscriptionBehavior);

            var subject = new Subject<IMessage>();
            int id = Interlocked.Increment(ref _idSequence);

            _destinationsById[id] = subject;

            var retVal =
                new RefCountStompDestination<TMessage>(
                    new StompDestination<TMessage>(_transport, destination, id, subject, subscriptionBehavior), () => OnDispose(destinationKey, id));

            return retVal;
        }
        
        private void OnDispose(DestinationKey destinationKey, int id)
        {
            object value;
            Subject<IMessage> subject;

            if (_destinationsById.TryRemove(id, out subject))
            {
                subject.Dispose();
            }
            
            _destinationsByName.TryRemove(destinationKey, out value);
        }

    }
}