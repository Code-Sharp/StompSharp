using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stomp2
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

        public void SendAsync(IOutgoingMessage message, Action whenDone)
        {
            _destination.SendAsync(message, whenDone);
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

        private long _messageSequence;
        private bool _subscribed;

        private readonly object _receiptActionsSyncRoot = new object();
        private readonly Queue<ReceiptAction> _receiptActions = new Queue<ReceiptAction>();

        public StompDestination(StompTransport transport, string destination, int id, IObservable<IMessage> incommingMessages)
        {
            _transport = transport;
            _destination = destination;
            _id = id;
            _incommingMessages = incommingMessages;

            _transport.IncommingMessages.GetObservable("RECEIPT").Subscribe(OnReceiptReceived);

            _incommingMessagesObservable =
                Observable.Create(new Func<IObserver<IMessage>, Task<IDisposable>>(RegisterToQueue))
                    .Publish()
                    .RefCount();
        }

        private async Task<IDisposable> RegisterToQueue(IObserver<IMessage> arg)
        {
            _subscribed = true;

            await _transport.SendMessage(
                    new MessageBuilder("SUBSCRIBE").Header("destination", _destination).Header("id", _id).WithoutBody());

            _incommingMessages.Subscribe(arg);

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

        private void OnReceiptReceived(IMessage receiptMessage)
        {
            var messageId = receiptMessage.Headers.Single(h => h.Key == "receipt-id").Value.ToString();
            long messageSequence;

            // Not ours
            if (!messageId.StartsWith(_destination + ".") ||
                !long.TryParse(messageId.Substring(_destination.Length + 1), out messageSequence))
            {
                return;
            }

            lock (_receiptActionsSyncRoot)
            {

                var nextReceipt = _receiptActions.Peek();

                while (nextReceipt.MessageSequence < messageSequence)
                {
                    _receiptActions.Dequeue();
                    nextReceipt = _receiptActions.Peek();
                    Console.WriteLine("Error?!");
                }

                if (nextReceipt.MessageSequence == messageSequence)
                {
                    nextReceipt.Callback();
                    _receiptActions.Dequeue();
                    return;
                }

                if (nextReceipt.MessageSequence > messageSequence)
                {
                    // WTF?!                
                }
            }
        }

        public  void SendAsync(IOutgoingMessage message, Action whenDone)
        {
            var currentSequence = Interlocked.Increment(ref _messageSequence);

            lock (_receiptActionsSyncRoot)
            {
                _receiptActions.Enqueue(new ReceiptAction(currentSequence, whenDone));    
            }
            
            _transport.SendMessage(new OutgoingMessageAdapter(message, _destination, currentSequence));
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