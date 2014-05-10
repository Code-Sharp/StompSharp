using System.Collections.Generic;
using System.Linq;

namespace Stomp2
{
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
}