namespace StompSharp.Messages
{
    /// <summary>
    /// A decorator for <see cref="IOutgoingMessage"/>
    /// that adds a "receipt" header.
    /// </summary>
    public class ReceiptHeaderDecorator : HeaderDecorator
    {
        private readonly string _destination;
        private readonly long _messageSequence;

        /// <summary>
        /// Creates an instance of <see cref="ReceiptHeaderDecorator"/>
        /// with the given parameters.
        /// 
        /// The header value will be the <paramref name="destination"/>
        /// and the <paramref name="messageSequence"/> joined with a period (".").
        /// </summary>
        /// <param name="message"></param>
        /// <param name="destination"></param>
        /// <param name="messageSequence"></param>
        public ReceiptHeaderDecorator(IOutgoingMessage message, string destination, long messageSequence) : base(message)
        {
            _destination = destination;
            _messageSequence = messageSequence;
        }

        protected override string HeaderName
        {
            get { return "receipt"; }
        }

        protected override object HeaderValue
        {
            get { return _destination + "." + _messageSequence; }
        }
    }
}