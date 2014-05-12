using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stomp2
{

    public class LateSubscriber<T> : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly ReplaySubject<T> _subject;

        public LateSubscriber(IObservable<T> observable)
        {
            _subject = new ReplaySubject<T>(int.MaxValue);
            _subscription = observable.Subscribe(_subject);
        }



        public IObservable<T> RecordedItems
        {
            get { return _subject; }
        }

        public void Dispose()
        {
            _subscription.Dispose();
            _subject.Dispose();
        }
    }

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
                 new MessageSerializerQueue(
                    new StreamMessageSerializer(new StreamWriter(_client.GetStream(), Encoding.ASCII)));

            _incommingMessageRouter = new MessageRouter(_incommingMessagesSubject);

            Task.Factory.StartNew(ReadLoop);

            using (var lateSubscriber = new LateSubscriber<IMessage>(IncommingMessages.GetObservable("CONNECTED")))
            {
                SendMessage(new MessageBuilder("CONNECT").Header("accept-version", 1.2).WithoutBody()).Wait();

                lateSubscriber.RecordedItems.FirstAsync().Wait();
            }
        }

        private void ReadLoop()
        {
            try
            {
                while (!_disposed)
                {
                    _incommingMessagesSubject.OnNext(_messageFactory.Create().Result);
                }
            }
            catch (Exception)
            {
                if (_disposed)
                {
                    return;
                }
                throw;
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
            await _messageSerializer.Serialize(message);
            _outgoingMessagesSubject.OnNext(message);
        }

        public async void Dispose()
        {
            // Register for receipt
            var subscription = IncommingMessages.GetObservable("RECEIPT")
                .Where(m => m.Headers.Any(h => h.Key == "receipt-id" && h.Value.ToString() == "disconnect-msg"));

            using (var lateSubscriber = new LateSubscriber<IMessage>(subscription))
            {
                // Dispose nicely, Send Discnonect message and wait for receipt.
                await SendMessage(new MessageBuilder("DISCONNECT").Header("receipt", "disconnect-msg").WithoutBody());

                lateSubscriber.RecordedItems.FirstAsync().Wait();
            }

            _disposed = true;

            _messageSerializer.Dispose();
            _incommingMessagesSubject.Dispose();
            _outgoingMessagesSubject.Dispose();
            _client.Close();
        }
    }
}