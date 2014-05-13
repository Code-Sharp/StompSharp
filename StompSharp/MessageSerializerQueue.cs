using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using StompSharp.Threading;

namespace StompSharp
{
    public class MessageSerializerQueue : IMessageSerializer
    {
        private readonly IMessageSerializer _next;

        private readonly TaskFactory _taskFactory =
            new TaskFactory(new BlockingCollectionTaskScheduler(new BlockingCollection<Task>(100)));

        public MessageSerializerQueue(IMessageSerializer next)
        {
            _next = next;
        }

        public Task Serialize(IMessage message)
        {
            return _taskFactory.StartNew(() => _next.Serialize(message)).Unwrap();
        }

        public void Dispose()
        {
            var disposableScheduler = _taskFactory.Scheduler as IDisposable;
            if (disposableScheduler != null)
            {
                disposableScheduler.Dispose();
            }

            _next.Dispose();
        }
    }
}