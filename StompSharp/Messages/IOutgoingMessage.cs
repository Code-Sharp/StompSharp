using System.Collections.Generic;

namespace StompSharp.Messages
{
    /// <summary>
    /// Represents an <see cref="IOutgoingMessage"/>.
    /// </summary>
    public interface IOutgoingMessage
    {
        /// <summary>
        /// The body of the message
        /// </summary>
        byte[] Body { get; }

        /// <summary>
        /// The headers of the message
        /// </summary>
        IEnumerable<IHeader> Headers { get; }

    }
}