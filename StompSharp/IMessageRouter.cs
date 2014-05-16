using System;
using StompSharp.Messages;

namespace StompSharp
{
    /// <summary>
    /// Represents an object that receives <see cref="IMessage"/>s,
    /// Allows to register to all of them, Or to some of them by a given command.
    /// </summary>
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