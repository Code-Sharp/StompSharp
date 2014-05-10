using System;

namespace Stomp2
{
    internal class ReceiptAction
    {
        private readonly long _messageSequence;
        private readonly Action _callback;

        public long MessageSequence
        {
            get { return _messageSequence; }
        }

        public void Callback()
        {
            if (_callback != null)
            {
                _callback();
            }
        }

        public ReceiptAction(long messageSequence, Action callback)
        {
            _messageSequence = messageSequence;
            _callback = callback;
        }
    }
}