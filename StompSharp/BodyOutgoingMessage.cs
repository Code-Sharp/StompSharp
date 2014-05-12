using System.Collections.Generic;
using System.Linq;

namespace Stomp2
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

    public class PersistentHeaderDecorator : HeaderDecorator
    {
        
        public PersistentHeaderDecorator(IOutgoingMessage child) : base(child)
        {
        }

        protected override string HeaderName
        {
            get { return "persistent"; }
        }

        protected override object HeaderValue
        {
            get { return "true"; }
        }
    }

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

    public static class TransactionExtensions
    {


        public static IOutgoingMessage WithTransaction(this IOutgoingMessage message, IStompTransaction transaction)
        {
            return new TransactionHeaderDecorator(message, transaction.Id);
        }

        public static IOutgoingMessage WithPersistence(this IOutgoingMessage message)
        {
            return new PersistentHeaderDecorator(message);
        }


    }

    internal class BodyOutgoingMessage : IOutgoingMessage
    {
        private readonly byte[] _body;

        public byte[] Body
        {
            get { return _body; }
        }

        public IEnumerable<IHeader> Headers
        {
            get
            {
                //yield return new Header("content-type", "application/octet-stream; charset=binary");
                return Enumerable.Empty<IHeader>();
            }
        }

        public BodyOutgoingMessage(byte[] body)
        {
            _body = body;
        }
    }
}