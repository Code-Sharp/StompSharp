using System.Threading.Tasks;

namespace StompSharp
{
    public interface IMessageFactory 
    {

        Task<IMessage> Create();

    }
}