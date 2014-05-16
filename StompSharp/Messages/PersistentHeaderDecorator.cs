namespace StompSharp.Messages
{
    /// <summary>
    /// A decorator for <see cref="IOutgoingMessage"/>
    /// that adds a "persistent" header (With the value "true").
    /// </summary>
    public class PersistentHeaderDecorator : HeaderDecorator
    {
        
        /// <summary>
        /// Creates an instance of <see cref="PersistentHeaderDecorator"/>
        /// that wraps the given <paramref name="child"/>.
        /// </summary>
        /// <param name="child"></param>
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