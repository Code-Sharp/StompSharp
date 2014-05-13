using System.Threading.Tasks;
using StompSharp.Messages;

namespace StompSharp
{
    public interface IMessageFactory 
    {

        Task<IMessage> Create();

    }
}