using System.Collections.Generic;

namespace StompSharp.Messages
{
    public interface IMessage
    {

        string Command { get; }

        IEnumerable<IHeader> Headers { get; }

        byte[] Body { get; }

    }
}