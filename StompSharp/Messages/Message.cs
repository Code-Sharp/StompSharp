using System;
using System.Collections.Generic;
using System.Text;

namespace StompSharp.Messages
{
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
}