using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using StompSharp.Messages;
using StompSharp.Transport;

namespace StompSharp
{
    public class RefCountStompDestination : IDestination
    {
        private readonly IDestination _destination;
        private readonly Action<RefCountStompDestination> _disposeAction;
        private int _references;
        

        public RefCountStompDestination(IDestination destination, Action<RefCountStompDestination> disposeAction)
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
                _disposeAction(this);
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

        public IObservable<IMessage> IncommingMessages
        {
            get { return _destination.IncommingMessages; }
        }
    }

    public class StompDestination : IDestination
    {
        private readonly StompTransport _transport;
        private readonly string _destination;
        private readonly int _id;
        private readonly IObservable<IMessage> _incommingMessagesObservable;
        private readonly IObservable<IMessage> _incommingMessages;
        private bool _subscribed;

        public StompDestination(StompTransport transport, string destination, int id, IObservable<IMessage> incommingMessages)
        {
            _transport = transport;
            _destination = destination;
            _id = id;
            _incommingMessages = incommingMessages;

            _incommingMessagesObservable =
                Observable.Create(new Func<IObserver<IMessage>, Task<IDisposable>>(RegisterToQueue))
                    .Publish()
                    .RefCount();
        }

        private async Task<IDisposable> RegisterToQueue(IObserver<IMessage> arg)
        {
            _subscribed = true;

            _incommingMessages.Subscribe(arg);

            await _transport.SendMessage(
                    new MessageBuilder("SUBSCRIBE").Header("destination", _destination).Header("id", _id).WithoutBody());

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

        public IObservable<IMessage> IncommingMessages
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