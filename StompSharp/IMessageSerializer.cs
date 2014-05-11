using System;
using System.Threading.Tasks;

namespace Stomp2
{
    public interface IMessageSerializer : IDisposable
    {
        Task Serialize(IMessage message);
    }
}