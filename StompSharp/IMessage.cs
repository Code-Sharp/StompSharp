using System.Collections.Generic;
using System.Threading.Tasks;

namespace StompSharp
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

    public interface IMessage
    {

        string Command { get; }

        IEnumerable<IHeader> Headers { get; }

        byte[] Body { get; }

    }
}