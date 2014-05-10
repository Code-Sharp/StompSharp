using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Stomp2
{
    public class StreamMessageSerializer : IMessageSerializer
    {
        private readonly StreamWriter _streamWriter;

        public StreamMessageSerializer(StreamWriter streamWriter)
        {
            _streamWriter = streamWriter;
            _streamWriter.AutoFlush = false;
        }

        public async Task Serialize(IMessage message)
        {
            // Command
            await _streamWriter.WriteLineAsync(message.Command).ConfigureAwait(false);

            //Headers
            char[] bodyBuffer = _streamWriter.Encoding.GetChars(message.Body);
            // Content-length header.
            await _streamWriter.WriteLineAsync(string.Format("content-length:{0}", bodyBuffer.Length)).ConfigureAwait(false);
            
            foreach (var header in message.Headers)
            {
                await _streamWriter.WriteLineAsync(string.Format("{0}:{1}", header.Key, header.Value)).ConfigureAwait(false);
            }

            await _streamWriter.WriteLineAsync().ConfigureAwait(false);

            // Body 
            await _streamWriter.WriteAsync(bodyBuffer).ConfigureAwait(false);

            // Null
            await _streamWriter.WriteLineAsync((char)0).ConfigureAwait(false);

            await _streamWriter.FlushAsync().ConfigureAwait(false);
        }
    }
}