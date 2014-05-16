using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp
{
    /// <summary>
    /// An <see cref="IReceiptBehavior"/>
    /// that adds a Receipt Header for every outgoing message
    /// and wait for the server to send receipt to that message.
    /// </summary>
    public class ReceiptBehavior : IReceiptBehavior
    {
        private readonly string _destination;
        private readonly object _receiptActionsSyncRoot = new object();
        private readonly Queue<ReceiptEvent> _receiptActions = new Queue<ReceiptEvent>();

        private long _messageSequence;
        private readonly IDisposable _subscription;

        /// <summary>
        /// Creates an instance of a <see cref="ReceiptBehavior"/>
        /// for the given destination.
        /// </summary>
        /// <param name="destination">The outgoing messages destination</param>
        /// <param name="messageRouter">The incomming messages router (To listen for Receipt messages).</param>
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
                    // Unbeliveable. TODO : Log         
                }
            }
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}