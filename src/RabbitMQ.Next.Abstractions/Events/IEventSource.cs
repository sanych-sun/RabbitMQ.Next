using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Abstractions.Events
{
    public interface IEventSource<out TEventArgs>
    {
        IDisposable Subscribe<TSubscriber>(TSubscriber subscriber, Func<TSubscriber, Func<TEventArgs, ValueTask>> handlerSelector)
            where TSubscriber: class;
    }
}