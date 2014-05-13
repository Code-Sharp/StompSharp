using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using StompSharp.Messages;

namespace StompSharp
{
    public class MessageRouter : IMessageRouter
    {
        private readonly IObservable<IMessage> _all;

        private readonly ConcurrentDictionary<string, Subject<IMessage>> _commandSubjects =
            new ConcurrentDictionary<string, Subject<IMessage>>(StringComparer.CurrentCultureIgnoreCase);

        public MessageRouter(IObservable<IMessage> all)
        {
            _all = all;
            _all.Subscribe(MessageReceived);
        }

        private void MessageReceived(IMessage obj)
        {
            _commandSubjects.GetOrAdd(obj.Command, CreateSubject).OnNext(obj);
        }

        public IObservable<IMessage> GetObservable(string command)
        {
            return _commandSubjects.GetOrAdd(command, CreateSubject);
        }
        private Subject<IMessage> CreateSubject(string arg)
        {
            return new Subject<IMessage>();
        }


        public IObservable<IMessage> All
        {
            get { return _all; }
        }
    }
}