using System.Collections.Generic;

namespace StompSharp.Messages
{
    /// <summary>
    /// A decorator that adds an header for <see cref="IOutgoingMessage"/>.
    /// </summary>
    public abstract class HeaderDecorator : IOutgoingMessage
    {
        private readonly IOutgoingMessage _child;

        /// <summary>
        /// Creates an instance of <see cref="HeaderDecorator"/>
        /// that wraps the given <paramref name="child"/>
        /// </summary>
        /// <param name="child"></param>
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

        /// <summary>
        /// Gets the header name that should be added to the message
        /// </summary>
        protected abstract string HeaderName { get; }

        /// <summary>
        /// Gets the header value for the given <see cref="HeaderName"/>.
        /// </summary>
        protected abstract object HeaderValue { get; }
    }
}