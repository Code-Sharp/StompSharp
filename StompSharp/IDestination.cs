using System;

namespace Stomp2
{
    public interface IDestination
    {
        /// <summary>
        /// Sends the given <paramref name="message"/>,
        /// And returns a task that will complete when the message
        /// is received on the server (Receipt).
        /// </summary>
        /// <param name="message"></param>
        void SendAsync(IOutgoingMessage message, Action whenDone);

        /// <summary>
        /// Gets an observable that subscribes
        /// to incomming messages for this destination.
        /// 
        /// (The subscription is lazy)
        /// </summary>
        IObservable<IMessage> IncommingMessages { get; }
    }
}