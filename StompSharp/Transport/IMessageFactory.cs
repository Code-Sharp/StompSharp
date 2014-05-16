using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp.Transport
{
    /// <summary>
    /// A factory that creates <see cref="IMessage"/>s
    /// </summary>
    public interface IMessageFactory 
    {

        /// <summary>
        /// Creates a message asynchronously.
        /// </summary>
        /// <returns></returns>
        Task<IMessage> Create();

    }
}