using System;
using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp.Transport
{
   
    /// <summary>
    /// Represents a logical connection to the otherside (Mostly a STOMP compatible server).
    /// </summary>
    public interface ITransport : IDisposable
    {

        /// <summary>
        /// Gets an observable with all of the incomming messages
        /// </summary>
        IMessageRouter IncommingMessages { get; }

        /// <summary>
        /// Gets an observable with all of the outgoing messages
        /// </summary>
        IObservable<IMessage> OutgoingMessages { get; }

        /// <summary>
        /// Sends the message synchronously to the server
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendMessage(IMessage message);


    }
}