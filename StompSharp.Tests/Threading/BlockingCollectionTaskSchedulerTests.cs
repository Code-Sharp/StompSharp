using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Stomp2.Threading;

namespace StompSharp.Tests.Threading
{
    [TestFixture]
    public class BlockingCollectionTaskSchedulerTests
    {

        [Test]
        public void Try()
        {
            var factory = new TaskFactory(new BlockingCollectionTaskScheduler(new BlockingCollection<Task>(10)));

            Task[] tasks = new Task[20];
            for (int i = 0; i < tasks.Length; i++)
            {
                int tempI = i;
                tasks[i] = factory.StartNew(DoSomething(i)).ContinueWith(task => Console.WriteLine(tempI + "Done!"));
            }

            Task.WaitAll(tasks);
        }


        private static Action DoSomething(int i)
        {
            return () =>
            {
                Console.WriteLine(i + " - Start");
                Thread.Sleep(500);
                Console.WriteLine(i + " - Stop");
            };
        }

    }
}
