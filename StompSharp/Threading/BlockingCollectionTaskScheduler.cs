using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Stomp2.Threading
{
    public class BlockingCollectionTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly BlockingCollection<Task> _collection;
        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public BlockingCollectionTaskScheduler(BlockingCollection<Task> collection)
        {
            _collection = collection;
            _thread = new Thread(RunTasks);
            _thread.Start();
        }

        private void RunTasks()
        {
            try
            {
                foreach (var task in _collection.GetConsumingEnumerable(_cancellationTokenSource.Token))
                {
                    TryExecuteTask(task);
                }
            }
            catch (OperationCanceledException)
            {
                // Mute OperationCanceledException (On dispose)
            }
        }

        protected override void QueueTask(Task task)
        {
            _collection.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _collection.GetConsumingEnumerable();
        }

        public void Dispose()
        {
            _collection.CompleteAdding();
            _cancellationTokenSource.Cancel();
            _thread.Join();
        }
    }
}
