using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Stomp2
{
    public class StompTransport : ITransport
    {
        private readonly Subject<IMessage> _incommingMessagesSubject = new Subject<IMessage>();
        private readonly Subject<IMessage> _outgoingMessagesSubject = new Subject<IMessage>();
        private readonly TcpClient _client;
        private readonly IMessageFactory _messageFactory;
        private readonly IMessageSerializer _messageSerializer;
        private bool _disposed;
        private readonly IMessageRouter _incommingMessageRouter;

        public StompTransport(string address, int port)
        {
            _client = new TcpClient();
            _client.Connect(address, port);

            _messageFactory = new StreamMessageFactory(new StreamReader(_client.GetStream(), Encoding.ASCII));
            _messageSerializer =
                // new MessageSerializerQueue(
                    new StreamMessageSerializer(new StreamWriter(_client.GetStream(), Encoding.ASCII));

            _incommingMessageRouter = new MessageRouter(_incommingMessagesSubject);

            Task.Factory.StartNew(ReadLoop);

            SendMessage(new MessageBuilder("CONNECT").Header("accept-version", 1.2).WithoutBody()).Wait();
        }

        private void ReadLoop()
        {
            while (!_disposed)
            {
                _incommingMessagesSubject.OnNext(_messageFactory.Create());
            }
        }

        public IMessageRouter IncommingMessages
        {
            get { return _incommingMessageRouter; }
        }

        public IObservable<IMessage> OutgoingMessages
        {
            get { return _outgoingMessagesSubject; }
        }

        public async Task SendMessage(IMessage message)
        {
            await _messageSerializer.Serialize(message).ConfigureAwait(false);
            _outgoingMessagesSubject.OnNext(message);
        }

        public void Dispose()
        {
            _disposed = true;

            _incommingMessagesSubject.Dispose();
            _outgoingMessagesSubject.Dispose();
            _client.Close();
        }
    }
}