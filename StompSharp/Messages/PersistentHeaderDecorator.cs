namespace StompSharp.Messages
{
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
}