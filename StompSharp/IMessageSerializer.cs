using System.Threading.Tasks;

namespace Stomp2
{
    public interface IMessageSerializer
    {
        Task Serialize(IMessage message);
    }
}