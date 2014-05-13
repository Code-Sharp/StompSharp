using System.Collections.Generic;

namespace StompSharp.Messages
{
    public class OutgoingMessageAdapter : IMessage
    {
        private readonly IOutgoingMessage _outgoingMessage;
        private readonly string _destination;
        
        public OutgoingMessageAdapter(IOutgoingMessage outgoingMessage, string destination)
        {
            _outgoingMessage = outgoingMessage;
            _destination = destination;
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

                yield return new Header("destination", _destination);
            }
        }

        public byte[] Body
        {
            get { return _outgoingMessage.Body; }
        }
    }
}