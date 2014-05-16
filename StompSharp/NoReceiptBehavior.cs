using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp
{
    /// <summary>
    /// An <see cref="IReceiptBehavior"/> that does not
    /// wait for a receipt (Meaning, Wait only for the server
    /// to receive the message).
    /// </summary>
    public class NoReceiptBehavior : IReceiptBehavior
    {
        /// <summary>
        /// Gets an instance of <see cref="NoReceiptBehavior"/>.
        /// </summary>
        public static readonly IReceiptBehavior Default = new NoReceiptBehavior();

        private NoReceiptBehavior()
        {
            
        }

        public Task DecorateSendMessageTask(Task sendMessageTask)
        {
            return sendMessageTask;
        }

        public IOutgoingMessage DecorateMessage(IOutgoingMessage message)
        {
            return message;
        }

        public void Dispose()
        {
            // Does nothing!
        }
    }
}