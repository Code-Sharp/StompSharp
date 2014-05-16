using System.IO;
using System.Threading.Tasks;
using StompSharp.Messages;
using StompSharp.Transport;

namespace StompSharp
{
    public class StreamMessageSerializer : IMessageSerializer
    {
        private readonly StreamWriter _streamWriter;

        public StreamMessageSerializer(StreamWriter streamWriter)
        {
            _streamWriter = streamWriter;
            _streamWriter.AutoFlush = false;
        }

        public Task Serialize(IMessage message)
        {
            // Command
            _streamWriter.WriteLine(message.Command);

            //Headers
            byte[] bodyBuffer = message.Body;
            // Content-length header.
            _streamWriter.WriteLine("content-length:{0}", bodyBuffer.Length);

            foreach (var header in message.Headers)
            {
                _streamWriter.WriteLine("{0}:{1}", header.Key, header.Value);
            }

            _streamWriter.WriteLine();
            _streamWriter.Flush();

            // Body 
            _streamWriter.BaseStream.Write(bodyBuffer, 0, bodyBuffer.Length);

            // Null
            _streamWriter.WriteLine((char) 0);

            _streamWriter.Flush();

            return Task.FromResult((object) null);
        }

        public void Dispose()
        {
            // Do nothing!
        }
    }
}