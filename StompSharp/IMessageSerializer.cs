using System;
using System.Threading.Tasks;

namespace StompSharp
{
    public interface IMessageSerializer : IDisposable
    {
        Task Serialize(IMessage message);
    }
}