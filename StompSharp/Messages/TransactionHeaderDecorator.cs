namespace StompSharp.Messages
{
    public class TransactionHeaderDecorator : HeaderDecorator
    {
        private readonly int _transactionId;

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