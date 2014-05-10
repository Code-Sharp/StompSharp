using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stomp2
{
    public class StompDestination : IDestination
    {
        private readonly StompTransport _transport;
        private readonly string _destination;
        private readonly int _id;
        private static long _messageSequence;

        private Queue<ReceiptAction> _receiptActions = new Queue<ReceiptAction>();

        private readonly IObservable<IMessage> _incommingMessagesObservable;

        public StompDestination(StompTransport transport, string destination, int id)
        {
            _transport = transport;
            _destination = destination;
            _id = id;

            _transport.IncommingMessages.GetObservable("RECEIPT").Subscribe(OnReceiptReceived);

            _incommingMessagesObservable =
                Observable.Create(new Func<IObserver<IMessage>, Task<IDisposable>>(RegisterToQueue));
        }

        private async Task<IDisposable> RegisterToQueue(IObserver<IMessage> arg)
        {
            await
                _transport.SendMessage(
                    new MessageBuilder("SUBSCRIBE").Header("destination", _destination).Header("id", _id).WithoutBody());

            _transport.IncommingMessages.GetObservable("MESSAGE")
                .Where(m => m.Headers.Any(h => h.Key == "id" && h.Value == _id.ToString()))
                .Subscribe(arg);

            return Disposable.Create(Unsubscribe);
        }


        private async void Unsubscribe()
        {
            await _transport.SendMessage(new MessageBuilder("UNSUBSCRIBE").Header("Id", _id).WithoutBody());
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

        public void SendAsync(IOutgoingMessage message, Action whenDone)
        {
            var currentSequence = Interlocked.Increment(ref _messageSequence);

            _receiptActions.Enqueue(new ReceiptAction(currentSequence, whenDone));
            _transport.SendMessage(new OutgoingMessageAdapter(message, _destination, currentSequence));
        }

        public IObservable<IMessage> IncommingMessages
        {
            get { return _incommingMessagesObservable; }
        }
    }
}