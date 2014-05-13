using System;
using System.Reactive.Subjects;

namespace StompSharp.Transport
{
    public class LateSubscriber<T> : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly ReplaySubject<T> _subject;

        public LateSubscriber(IObservable<T> observable)
        {
            _subject = new ReplaySubject<T>(int.MaxValue);
            _subscription = observable.Subscribe(_subject);
        }



        public IObservable<T> RecordedItems
        {
            get { return _subject; }
        }

        public void Dispose()
        {
            _subscription.Dispose();
            _subject.Dispose();
        }
    }
}