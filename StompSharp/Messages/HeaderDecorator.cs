using System.Collections.Generic;

namespace StompSharp.Messages
{
    public abstract class HeaderDecorator : IOutgoingMessage
    {
        private readonly IOutgoingMessage _child;

        protected HeaderDecorator(IOutgoingMessage child)
        {
            _child = child;
        }

        public IEnumerable<IHeader> Headers
        {
            get
            {
                foreach (var header in _child.Headers)
                {
                    yield return header;

                }

                yield return new Header(HeaderName, HeaderValue);
            }
        }

        public byte[] Body
        {
            get { return _child.Body; }
        }

        protected abstract string HeaderName { get; }

        protected abstract object HeaderValue { get; }
    }
}