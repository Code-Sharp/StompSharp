namespace StompSharp.Messages
{
 
    /// <summary>
    /// A decorator for <see cref="IOutgoingMessage"/>
    /// that adds a transaction header.
    /// </summary>
    public class TransactionHeaderDecorator : HeaderDecorator
    {
        private readonly int _transactionId;

        /// <summary>
        /// Creates an instance of <see cref="TransactionHeaderDecorator"/>
        /// for the given <paramref name="child"/> and <paramref name="transactionId"/>.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="transactionId"></param>
        public TransactionHeaderDecorator(IOutgoingMessage child, int transactionId) : base(child)
        {
            _transactionId = transactionId;
        }

        protected override string HeaderName
        {
            get { return "transaction"; }
        }

        protected override object HeaderValue
        {
            get { return _transactionId; }
        }
    }
}