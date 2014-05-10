using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stomp2
{
    class StreamMessageFactory : IMessageFactory
    {
        private readonly StreamReader _reader;

        public StreamMessageFactory(StreamReader reader)
        {
            _reader = reader;
        }


        public IMessage Create()
        {
            string command = "\0";
            while (command == "\0" || command == string.Empty)
            {
                command = _reader.ReadLine();
            }

            var headers = GetHeaders();

            var contentLength =
                headers.Where(k => k.Key == "content-length")
                    .Select(k => int.Parse(k.Value.ToString()))
                    .DefaultIfEmpty(-1)
                    .FirstOrDefault();

            // Do something about it!
            if (contentLength != -1)
            {
                char[] bodyBuffer = new char[contentLength];

                var totalBytes = 0;
                do
                {
                    var readBytes = _reader.Read(bodyBuffer, totalBytes, bodyBuffer.Length - totalBytes);
                    totalBytes += readBytes;
                } while (totalBytes != contentLength);
                

                // Read the last Null.
                byte nullByte = (byte) _reader.Read();

                if (nullByte != (char) 0)
                {
                    throw new Exception();
                }



                return new Message(command, headers, CastToByteBuffer(bodyBuffer));
            }

            // Read until null is found.
            IList<byte> body = new List<byte>();

            var readByte = _reader.Read();
            while (readByte != 0)
            {
                body.Add((byte)readByte);

                readByte = _reader.BaseStream.ReadByte();
            }

            return new Message(command, headers, body.ToArray());
        }

        private byte[] CastToByteBuffer(char[] bodyBuffer)
        {
            byte[] retVal = new byte[bodyBuffer.Length];

            for (int i = 0; i < bodyBuffer.Length; i++)
            {
                retVal[i] = (byte)bodyBuffer[i];
            }

            return retVal;
        }

        private IList<IHeader> GetHeaders()
        {
            IList<IHeader> headers = new List<IHeader>();
            var headerLine = _reader.ReadLine();
            while (headerLine != string.Empty)
            {
                var separatorIndex = headerLine.IndexOf(':');

                var key = headerLine.Substring(0, separatorIndex);
                var value = headerLine.Substring(separatorIndex + 1);

                headers.Add(new Header(key, value));

                headerLine = _reader.ReadLine();
            }
            return headers;
        }
    }
}