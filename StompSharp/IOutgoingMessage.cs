using System.Collections.Generic;

namespace StompSharp
{
    public interface IOutgoingMessage
    {
        byte[] Body { get; }

        IEnumerable<IHeader> Headers { get; }

    }
}