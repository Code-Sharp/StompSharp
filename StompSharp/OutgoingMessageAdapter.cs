using System.Collections.Generic;

namespace StompSharp
{
    public class OutgoingMessageAdapter : IMessage
    {
        private readonly IOutgoingMessage _outgoingMessage;
        private readonly string _destination;
        private readonly long _messageSequence;

        public OutgoingMessageAdapter(IOutgoingMessage outgoingMessage, string destination, long messageSequence)
        {
            _outgoingMessage = outgoingMessage;
            _destination = destination;
            _messageSequence = messageSequence;
        }

        public string Command
        {
            get { return "SEND"; }
        }

        public IEnumerable<IHeader> Headers
        {
            get
            {
                foreach (var header in _outgoingMessage.Headers)
                {
                    yield return header;
                }

                yield return new Header("receipt", _destination + "." + _messageSequence);
                yield return new Header("destination", _destination);
            }
        }

        public byte[] Body
        {
            get { return _outgoingMessage.Body; }
        }
    }
}