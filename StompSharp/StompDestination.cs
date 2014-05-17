using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using StompSharp.Messages;
using StompSharp.Transport;

namespace StompSharp
{
    public class RefCountStompDestination<TMessage> : IDestination<TMessage>
        where TMessage : IMessage
    {
        private readonly IDestination<TMessage> _destination;
        private readonly Action _disposeAction;
        private int _references;
        

        public RefCountStompDestination(IDestination<TMessage> destination, Action disposeAction)
        {
            _destination = destination;
            _disposeAction = disposeAction;
            _references = 0;
        }

        public void Dispose()
        {
            if (Interlocked.Decrement(ref _references) == 0)
            {
                _destination.Dispose();
                _disposeAction();
            }
        }

        public void Increment()
        {
            Interlocked.Increment(ref _references);
        }

        public string Destination
        {
            get { return _destination.Destination; }
        }

        public int Id
        {
            get { return _destination.Id; }
        }

        public Task SendAsync(IOutgoingMessage message, IReceiptBehavior receiptBehavior)
        {
            return _destination.SendAsync(message, receiptBehavior);
        }

        public IObservable<TMessage> IncommingMessages
        {
            get { return _destination.IncommingMessages; }
        }
    }

    public class StompDestination<TMessage> : IDestination<TMessage> 
        where TMessage : IMessage
    {
        private readonly ITransport _transport;
        private readonly string _destination;
        private readonly int _id;
        private readonly IObservable<IMessage> _incommingMessages;
        private readonly ISubscriptionBehavior<TMessage> _subscriptionBehavior;
        private readonly IObservable<TMessage> _incommingMessagesObservable;
        private bool _subscribed;

        public StompDestination(ITransport transport, string destination, int id, IObservable<IMessage> incommingMessages, ISubscriptionBehavior<TMessage> subscriptionBehavior)
        {
            _transport = transport;
            _destination = destination;
            _id = id;
            _incommingMessages = incommingMessages;
            _subscriptionBehavior = subscriptionBehavior;

            _incommingMessagesObservable =
                Observable.Create(new Func<IObserver<IMessage>, Task<IDisposable>>(RegisterToQueue))
                    .Select(_subscriptionBehavior.Transform)
                    .Publish()
                    .RefCount();
        }

        private async Task<IDisposable> RegisterToQueue(IObserver<IMessage> arg)
        {
            _subscribed = true;

            _incommingMessages.Subscribe(arg);

            await _transport.SendMessage(
                    _subscriptionBehavior.GetSubscriptionMessage(Destination, Id)
                    );

            return Disposable.Create(Unsubscribe);
        }


        private async void Unsubscribe()
        {
            if (_subscribed)
            {
                await _transport.SendMessage(new MessageBuilder("UNSUBSCRIBE").Header("ID", _id).Header("destination", _destination).WithoutBody());
                _subscribed = false;    
            }
        }

        public Task SendAsync(IOutgoingMessage message, IReceiptBehavior receiptBehavior)
        {
            return
                receiptBehavior.DecorateSendMessageTask(
                    _transport.SendMessage(new OutgoingMessageAdapter(receiptBehavior.DecorateMessage(message),
                        _destination)));
        }

        public IObservable<TMessage> IncommingMessages
        {
            get { return _incommingMessagesObservable; }
        }

        public string Destination
        {
            get { return _destination; }
        }

        public int Id
        {
            get { return _id; }
        }

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}