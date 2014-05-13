using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp
{
    public interface IReceiptBehavior : IDisposable
    {

        Task DecorateSendMessageTask(Task sendMessageTask);

        IOutgoingMessage DecorateMessage(IOutgoingMessage message);


    }

    public class ReceiptBehavior : IReceiptBehavior
    {
        private readonly string _destination;
        private readonly object _receiptActionsSyncRoot = new object();
        private readonly Queue<ReceiptEvent> _receiptActions = new Queue<ReceiptEvent>();

        private long _messageSequence;
        private readonly IDisposable _subscription;


        public ReceiptBehavior(string destination, IMessageRouter messageRouter)
        {
            _destination = destination;
            _subscription = messageRouter.GetObservable("RECEIPT").Subscribe(OnReceiptReceived);
        }

        public Task DecorateSendMessageTask(Task sendMessageTask)
        {
            var currentSequence = _messageSequence;

            var resetEvent = new ManualResetEvent(false);

            lock (_receiptActionsSyncRoot)
            {
                _receiptActions.Enqueue(new ReceiptEvent(currentSequence, resetEvent));
            }

            return sendMessageTask.ContinueWith(t => resetEvent.Set());
        }

        public IOutgoingMessage DecorateMessage(IOutgoingMessage message)
        {
            return new ReceiptHeaderDecorator(message, _destination, Interlocked.Increment(ref _messageSequence));
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

        public void Dispose()
        {
            _subscription.Dispose();

        }
    }

    internal class ReceiptHeaderDecorator : HeaderDecorator
    {
        private readonly string _destination;
        private readonly long _messageSequence;

        public ReceiptHeaderDecorator(IOutgoingMessage message, string destination, long messageSequence) : base(message)
        {
            _destination = destination;
            _messageSequence = messageSequence;
        }

        protected override string HeaderName
        {
            get { return "receipt"; }
        }

        protected override object HeaderValue
        {
            get { return _destination + "." + _messageSequence; }
        }
    }

    public class NoReceiptBehavior : IReceiptBehavior
    {
        public static readonly IReceiptBehavior Default = new NoReceiptBehavior();

        private NoReceiptBehavior()
        {
            
        }

        public Task DecorateSendMessageTask(Task sendMessageTask)
        {
            return sendMessageTask;
        }

        public IOutgoingMessage DecorateMessage(IOutgoingMessage message)
        {
            return message;
        }

        public void Dispose()
        {
            // Does nothing!
        }
    }

}