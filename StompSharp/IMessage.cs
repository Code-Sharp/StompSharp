using System.Collections.Generic;
using System.IO;

namespace Stomp2
{
    public interface IMessage
    {

        string Command { get; }

        IEnumerable<IHeader> Headers { get; }

        byte[] Body { get; }

    }
}