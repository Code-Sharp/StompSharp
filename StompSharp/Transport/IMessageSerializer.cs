using System;
using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp.Transport
{
    /// <summary>
    /// A (asynchronous) serializer of <see cref="IMessage"/>s.
    /// </summary>
    public interface IMessageSerializer : IDisposable
    {
        /// <summary>
        /// Serializes the given <paramref name="message"/> asynchronously.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Serialize(IMessage message);
    }
}