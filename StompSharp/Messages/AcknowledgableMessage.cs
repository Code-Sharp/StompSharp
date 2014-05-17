using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StompSharp.Transport;

namespace StompSharp.Messages
{
    internal class AcknowledgableMessage : IAcknowledgableMessage
    {
        private readonly IMessage _message;
        private readonly ITransport _transport;

        public AcknowledgableMessage(IMessage message, ITransport transport)
        {
            _message = message;
            _transport = transport;
        }

        public string Command
        {
            get { return _message.Command; }
        }

        public IEnumerable<IHeader> Headers
        {
            get { return _message.Headers; }
        }

        public byte[] Body
        {
            get { return _message.Body; }
        }

        private object GetAckId()
        {
            return Headers.First(h => h.Key == "ack").Value;
        }

        public Task Ack(IStompTransaction transaction = null)
        {
            return SendMessage("ACK", transaction);
        }

        public Task Nack(IStompTransaction transaction = null)
        {
            return SendMessage("NACK", transaction);
        }

        private Task SendMessage(string command, IStompTransaction transaction)
        {
            var builder = new MessageBuilder(command);
            builder.Header("id", GetAckId());

            if (transaction != null)
            {
                builder.Header("transaction", transaction.Id);
            }

            return _transport.SendMessage(builder.WithoutBody());
        }
    }
}