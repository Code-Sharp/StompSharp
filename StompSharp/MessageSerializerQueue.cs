using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using Stomp2.Threading;

namespace Stomp2
{
    public class MessageSerializerQueue : IMessageSerializer
    {
        private readonly IMessageSerializer _next;

        private readonly TaskFactory _taskFactory =
            //new TaskFactory(new QueuedTaskScheduler(1));
            new TaskFactory(new BlockingCollectionTaskScheduler(new BlockingCollection<Task>(1024)));

        public MessageSerializerQueue(IMessageSerializer next)
        {
            _next = next;
        }

        public Task Serialize(IMessage message)
        {
            return _taskFactory.StartNew(() => _next.Serialize(message)).Unwrap();
        }
    }
}