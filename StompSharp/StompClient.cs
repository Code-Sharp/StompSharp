using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using StompSharp.Messages;
using StompSharp.Transport;

namespace StompSharp
{
    public class StompClient : IStompClient
    {
        private readonly StompTransport _transport;
        private readonly IDestinationStorage _destinationStorage;
        private readonly ISubscriptionBehaviorProvider _subscriptionBehaviors;

        private int _transactionId;

        public StompClient(string address, int port)
        {
            _transport = new StompTransport(address, port);
            _destinationStorage = new DestinationStorage(_transport);
            _subscriptionBehaviors = new SubscriptionBehaviorProvider(_transport);
        }

        public void Dispose()
        {
            _transport.Dispose();
        }

        public ITransport Transport
        {
            get { return _transport; }
        }


        public IDestination<TMessage> GetDestination<TMessage>(string destination, ISubscriptionBehavior<TMessage> subscriptionBehavior) 
            where TMessage : IMessage
        {
            return _destinationStorage.Get(destination, subscriptionBehavior);
        }

        public Task<IStompTransaction> BeginTransaction()
        {
            var message = new MessageBuilder("BEGIN").Header("transaction", Interlocked.Increment(ref _transactionId)).WithoutBody();

            return
                _transport.SendMessage(message)
                    .ContinueWith<IStompTransaction>(m => new StompTransaction(_transactionId, Transport));
        }

        public ISubscriptionBehaviorProvider SubscriptionBehaviors
        {
            get { return _subscriptionBehaviors; }
        }
    }

    public class StompTransaction : IStompTransaction
    {
        private int _done = 0;
        private readonly int _transactionId;
        private readonly ITransport _transport;

        public StompTransaction(int transactionId, ITransport transport)
        {
            _transactionId = transactionId;
            _transport = transport;
        }

        public int Id
        {
            get { return _transactionId; }
        }

        public Task Commit()
        {
            if (Interlocked.CompareExchange(ref _done, 1, 0) == 0)
            {
                return _transport.SendMessage(GetMessage("COMMIT"));    
            }
            
            throw new Exception("This transaction has already done.");
        }

        public Task Rollback()
        {
            Task rollback;
            if (TryRollback(out rollback)) return rollback;

            throw new Exception("This transaction has already done.");
        }

        private bool TryRollback(out Task rollback)
        {
            if (Interlocked.CompareExchange(ref _done, 1, 0) == 0)
            {
                rollback = _transport.SendMessage(GetMessage("ABORT"));
                return true;
            }
            rollback = null;
            return false;
        }

        private IMessage GetMessage(string command)
        {
            return new MessageBuilder(command).Header("transaction", _transactionId).WithoutBody();
        }

        public void Dispose()
        {
            Task task;
            if (TryRollback(out task))
            {
                // What should we do? Wait?
            }

        }
    }
}