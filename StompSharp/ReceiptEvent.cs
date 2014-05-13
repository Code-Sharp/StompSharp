using System;
using System.Threading;

namespace StompSharp
{
    internal class ReceiptEvent
    {
        private readonly long _messageSequence;
        private readonly ManualResetEvent _callback;

        public long MessageSequence
        {
            get { return _messageSequence; }
        }

        public void Callback()
        {
            _callback.Set();
        }

        public ReceiptEvent(long messageSequence, ManualResetEvent callback)
        {
            _messageSequence = messageSequence;
            _callback = callback;
        }
    }
}