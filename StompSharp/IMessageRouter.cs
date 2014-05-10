using System;

namespace Stomp2
{
    public interface IMessageRouter
    {

        IObservable<IMessage> GetObservable(string command);

        IObservable<IMessage> All { get; }

    }
}