using System;
using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp
{
    /// <summary>
    /// A behavior that defines
    /// the completion of sending a message.
    /// 
    /// Two default implementations : <see cref="ReceiptBehavior"/>,
    /// <see cref="NoReceiptBehavior"/>.
    /// </summary>
    public interface IReceiptBehavior : IDisposable
    {

        /// <summary>
        /// Decorates the task that sends the message
        /// The task that returned here is the task that
        /// returned from <see cref="IDestination.SendAsync"/>.
        /// </summary>
        /// <param name="sendMessageTask"></param>
        /// <returns></returns>
        Task DecorateSendMessageTask(Task sendMessageTask);

        /// <summary>
        /// Decorates the outgoing messages,
        /// Mostly a good time to add headers.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IOutgoingMessage DecorateMessage(IOutgoingMessage message);


    }
}