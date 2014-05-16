using System;
using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp
{
    /// <summary>
    /// Represents a destination (Mostly, A queue or a topic, Or a composition of one or more of them)
    /// inside the STOMP server, Allows us to send or to subscribe for messages.
    /// </summary>
    public interface IDestination : IDisposable
    {
        /// <summary>
        /// Gets the destination name (That used to build this destination
        /// </summary>
        string Destination { get; }

        /// <summary>
        /// Gets the destination id (That given by <see cref="IStompClient"/>
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Sends the given <paramref name="message"/>,
        /// And returns a task that will complete according to the receipt behavior given.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="receiptBehavior"></param>
        Task SendAsync(IOutgoingMessage message, IReceiptBehavior receiptBehavior);

        /// <summary>
        /// Gets an observable that subscribes
        /// to incomming messages for this destination.
        /// 
        /// (The subscription is lazy)
        /// </summary>
        IObservable<IMessage> IncommingMessages { get; }

    }

}