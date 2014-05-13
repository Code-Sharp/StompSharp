using System;
using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp
{
    public interface IMessageSerializer : IDisposable
    {
        Task Serialize(IMessage message);
    }
}