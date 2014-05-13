using System.Collections.Generic;

namespace StompSharp.Messages
{
    public interface IOutgoingMessage
    {
        byte[] Body { get; }

        IEnumerable<IHeader> Headers { get; }

    }
}