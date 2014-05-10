using System.Collections.Generic;

namespace Stomp2
{
    public interface IOutgoingMessage
    {
        byte[] Body { get; }

        IEnumerable<IHeader> Headers { get; }

    }
}