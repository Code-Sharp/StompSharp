using System;

namespace Stomp2
{
    public interface IStompClient : IDisposable
    {

        /// <summary>
        /// Gets the transport that used to 
        /// make communication with the other side.
        /// </summary>
        ITransport Transport { get; }

        IDestination GetDestination(string destination);

    }
}