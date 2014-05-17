using System.Threading.Tasks;

namespace StompSharp.Messages
{
    /// <summary>
    /// Represents an acknowledgable incomming message.
    /// 
    /// This interface extends the <see cref="IMessage"/> interface
    /// with two methods that allows to Acknowledge/Deacknowledge the message.
    /// </summary>
    public interface IAcknowledgableMessage : IMessage
    {

        /// <summary>
        /// Acknowledges the message.
        /// 
        /// Removes the message from the original queue.
        /// </summary>
        /// <returns></returns>
        Task Ack(IStompTransaction transaction = null);

        /// <summary>
        /// Deacknowledges the message.
        /// 
        /// Returns the message to the original queue.
        /// </summary>
        /// <returns></returns>
        Task Nack(IStompTransaction transaction = null);

    }
}