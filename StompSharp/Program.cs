using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stomp2
{
    public class StompClient : IStompClient
    {
        private readonly TcpClient _client;
        private readonly object _syncRoot = new object();
        private readonly StreamWriter _writer;
        private readonly IMessageSerializer _messageSerializer;

        private readonly Subject<IMessage> _incommingMessages = new Subject<IMessage>();
        private bool _disposed;


        public StompClient(string address, int port)
        {
            _client = new TcpClient();
            _client.Connect(address, port);

            _writer = new StreamWriter(_client.GetStream(), Encoding.ASCII);
            _messageSerializer = new MessageSerializer();
        }

        private void WriteMessageSync(IMessage message)
        {
            lock (_syncRoot)
            {
                _messageSerializer.Serialize(message, _writer);
            }
        }

        private void ReadMessages()
        {
            var reader = new StreamReader(_client.GetStream(), Encoding.ASCII);
            var messageFactory = new MessageFactory();

            while (!_disposed)
            {
                _incommingMessages.OnNext(messageFactory.Create(reader));
            }
        }

        public IObservable<IMessage> Subscribe(string destination)
        {
            // TODO : Count check... :O

            return Observable.Create<IMessage>(o =>
            {
                // Register incomming messages with specific destination
                _incommingMessages.Where(
                    i =>
                        i.Command == "MESSAGE" &&
                        i.Headers.FirstOrDefault(h => h.Key == "destination" && h.Value.ToString() == destination) !=
                        null).
                    Subscribe(o);

                WriteMessageSync(new MessageBuilder("SUBSCRIBE").Header("destination", destination).WithoutBody());

                // Unsubscribe :)
                return Disposable.Create(() => WriteMessageSync(new MessageBuilder("UNSUBSCRIBE").Header("destination", destination).WithoutBody()));
            });
        }

        public IPublisher GetPublisher(string destination)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }

    public interface IStompClient : IDisposable
    {

        /// <summary>
        /// Gets the transport that used to 
        /// make communication with the other side.
        /// </summary>
        ITransport Transport { get; }



        /// <summary>
        /// Subscribes to the given destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        IObservable<IMessage> Subscribe(string destination);

        /// <summary>
        /// Gets the publisher to the given destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        IPublisher GetPublisher(string destination);

    }

    /// <lyrics>
    /// How long, how long will I slide?
    /// Separate my side
    /// I don't, I don't believe it's bad
    /// Slittin' my throat, it's all I ever
    /// </lyrics>
    
    public interface ITransport
    {

        IObservable<IMessage> IncommingMessages { get; }

        IObservable<IMessage> OutgoingMessages { get; }

        void SendMessage(IMessage message);

    }



    public interface IPublisher : IDisposable
    {

        /// <summary>
        /// Sends the given <paramref name="message"/>
        /// and calls the given <paramref name="whenDone"/>
        /// when a receipt is given.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="whenDone"></param>
        void SendAsync(IOutgoingMessage message, Action whenDone);

    }


    public interface IOutgoingMessage
    {
        byte[] Body { get; }

        IEnumerable<IHeader> Headers { get; }

    }

    class Program
    {
        static void Main(string[] args)
        {
            // Stomp connection
            TcpClient client = new TcpClient();

            client.Connect("localhost", 61613);

            Task.Factory.StartNew(() =>
            {
                var reader = new StreamReader(client.GetStream(), Encoding.ASCII);
                var messageFactory = new MessageFactory();
                while (true)
                {
                    Console.WriteLine(messageFactory.Create(reader));
                }
            });

            var writer = new StreamWriter(client.GetStream(), Encoding.ASCII);
            var messageSerializer = new MessageSerializer();

            messageSerializer.Serialize(new MessageBuilder("CONNECT").Header("accept-version", "1.2").WithoutBody(),
                writer);

            messageSerializer.Serialize(new MessageBuilder("SUBSCRIBE").Header("id", 1).Header("destination", "/queue/a").WithoutBody(),
                writer);

            for (int i = 0; i < 1000; i++)
            {
                messageSerializer.Serialize(
                new MessageBuilder("SEND").Header("destination", "/queue/a")
                    .Header("receipt", "message-" + i)
                    .Header("inner-id", i)
                    .WithBody(Encoding.UTF8.GetBytes("Hello World")), writer);

            }

            Console.ReadLine();
        }
    }

    public class MessageBuilder
    {
        private readonly string _command;

        private readonly IList<KeyValuePair<string, object>> _headers = new List<KeyValuePair<string, object>>();

        public MessageBuilder(string command)
        {
            _command = command;
        }

        public MessageBuilder Header(string key, object value)
        {
            _headers.Add(new KeyValuePair<string, object>(key, value));
            return this;
        }

        public IMessage WithBody(byte[] body)
        {
            return new Message(_command, GetHeaders(), body);
        }

        public IMessage WithoutBody()
        {
            return new Message(_command, GetHeaders(), new byte[0]);
        }

        private IList<IHeader> GetHeaders()
        {
            return _headers.Select(kvp => new Header(kvp.Key, kvp.Value))
                           .ToList<IHeader>();
        }
    }

    public interface IMessageFactory
    {

        IMessage Create(StreamReader reader);

    }

    public interface IMessageSerializer
    {
        void Serialize(IMessage message, StreamWriter writer);
    }

    public class MessageSerializer : IMessageSerializer
    {
        public void Serialize(IMessage message, StreamWriter writer)
        {
            // Command
            writer.WriteLine(message.Command);

            //Headers
            foreach (var header in message.Headers)
            {
                writer.WriteLine("{0}:{1}", header.Key, header.Value);
            }

            char[] bodyBuffer = writer.Encoding.GetChars(message.Body);
            // Content-length header.
            writer.WriteLine("content-length:{0}", bodyBuffer.Length);
            writer.WriteLine();

            // Body 
            writer.Write(bodyBuffer);

            // Null
            writer.WriteLine((char)0);

            writer.Flush();

        }
    }

    class MessageFactory : IMessageFactory
    {



        public IMessage Create(StreamReader reader)
        {
            string command = "\0";
            while (command == "\0" || command == string.Empty)
            {
                command = reader.ReadLine();
            }

            var headers = GetHeaders(reader);

            var contentLength =
                headers.Where(k => k.Key == "content-length")
                    .Select(k => int.Parse(k.Value.ToString()))
                    .DefaultIfEmpty(-1)
                    .FirstOrDefault();

            // Do something about it!
            if (contentLength != -1)
            {
                var bodyBuffer = new char[contentLength];

                reader.Read(bodyBuffer, 0, bodyBuffer.Length);

                // Read the last Null.
                reader.Read();

                return new Message(command, headers, reader.CurrentEncoding.GetBytes(bodyBuffer));
            }

            // Read until null is found.
            IList<byte> body = new List<byte>();

            var readByte = reader.Read();
            while (readByte != 0)
            {
                body.Add((byte)readByte);

                readByte = reader.BaseStream.ReadByte();
            }

            return new Message(command, headers, body.ToArray());
        }

        private static IList<IHeader> GetHeaders(StreamReader reader)
        {
            IList<IHeader> headers = new List<IHeader>();
            var headerLine = reader.ReadLine();
            while (headerLine != string.Empty)
            {
                var separatorIndex = headerLine.IndexOf(':');

                var key = headerLine.Substring(0, separatorIndex);
                var value = headerLine.Substring(separatorIndex + 1);

                headers.Add(new Header(key, value));

                headerLine = reader.ReadLine();
            }
            return headers;
        }
    }

    internal class Message : IMessage
    {
        private readonly string _command;
        private readonly IList<IHeader> _headers;
        private readonly byte[] _body;

        public Message(string command, IList<IHeader> headers, byte[] body)
        {
            _command = command;
            _headers = headers;
            _body = body;
        }

        public string Command
        {
            get { return _command; }
        }

        public IEnumerable<IHeader> Headers
        {
            get { return _headers; }
        }

        public byte[] Body
        {
            get { return _body; }
        }

        public override string ToString()
        {
            return Command + Environment.NewLine + string.Join(Environment.NewLine, Headers) + Environment.NewLine +
                   Environment.NewLine + Encoding.ASCII.GetString(Body) + "^@";
        }
    }

    internal class Header : IHeader
    {
        private readonly string _key;
        private readonly object _value;

        public Header(string key, object value)
        {
            _key = key;
            _value = value;
        }

        public string Key
        {
            get { return _key; }
        }

        public object Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Key, Value);
        }
    }
}
