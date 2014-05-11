using System.Threading.Tasks;

namespace Stomp2
{
    public interface IMessageFactory 
    {

        Task<IMessage> Create();

    }
}