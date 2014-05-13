using System;

namespace Stomp2
{
    public interface IMessageRouter
    {

        /// <summary>
        /// Gets all the messages with the specific given
        /// <paramref name="command"/>
        /// </summary>
        /// <param name="command">A command to filter the messages by</param>
        /// <returns></returns>
        IObservable<IMessage> GetObservable(string command);

        /// <summary>
        /// Gets all the messages that are routed via this
        /// instance of <see cref="IMessageRouter"/>.
        /// </summary>
        IObservable<IMessage> All { get; }

    }
}