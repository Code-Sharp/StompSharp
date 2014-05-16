using System;
using System.Threading.Tasks;
using StompSharp.Transport;

namespace StompSharp
{
    /// <summary>
    /// Represents a client for a STOMP compatible server.
    /// </summary>
    public interface IStompClient : IDisposable
    {

        /// <summary>
        /// Gets the transport that used to 
        /// make communication with the other side.
        /// </summary>
        ITransport Transport { get; }

        /// <summary>
        /// Gets the <paramref name="destination"/> destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        IDestination GetDestination(string destination);

        /// <summary>
        /// Gets a transaction
        /// </summary>
        /// <returns></returns>
        Task<IStompTransaction> GetTransaction();

    }

}