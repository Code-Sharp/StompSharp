using System.Threading.Tasks;

namespace StompSharp.Messages
{
    public interface IIncommingMessage : IMessage
    {

        /// <summary>
        /// Acknowledges the message.
        /// 
        /// Removes the message from the original queue.
        /// </summary>
        /// <returns></returns>
        Task Ack();

        /// <summary>
        /// Deacknowledges the message.
        /// 
        /// Returns the message to the original queue.
        /// </summary>
        /// <returns></returns>
        Task Nack();

    }
}