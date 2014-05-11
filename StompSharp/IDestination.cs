using System;
using System.Threading.Tasks;

namespace Stomp2
{
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
        /// And returns a task that will complete when the message is arrived to the server.
        /// 
        /// Also, <paramref name="whenDone"/> is called when a receipt
        /// is returned from the server.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="whenDone"></param>
        Task SendAsync(IOutgoingMessage message, Action whenDone);

        /// <summary>
        /// Gets an observable that subscribes
        /// to incomming messages for this destination.
        /// 
        /// (The subscription is lazy)
        /// </summary>
        IObservable<IMessage> IncommingMessages { get; }
    }
}