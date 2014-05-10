using System;
using System.Threading.Tasks;

namespace Stomp2
{
    /// <lyrics>
    /// How long, how long will I slide?
    /// Separate my side
    /// I don't, I don't believe it's bad
    /// Slittin' my throat, it's all I ever
    /// </lyrics>

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