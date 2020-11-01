using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Events
{
    internal class Subscription<TSubscriber, TEventArgs> : ISubscription<TEventArgs>, IDisposable
        where TSubscriber : class
    {
        private readonly WeakReference<TSubscriber> subscriber;
        private readonly Func<TSubscriber, Func<TEventArgs, ValueTask>> handlerSelector;

        public Subscription(TSubscriber subscriber, Func<TSubscriber, Func<TEventArgs, ValueTask>> handlerSelector)
        {
            this.subscriber = new WeakReference<TSubscriber>(subscriber);
            this.handlerSelector = handlerSelector;
        }

        public void Dispose()
        {
            this.subscriber.SetTarget(null);
        }

        public async ValueTask<bool> HandleAsync(TEventArgs eventArgs)
        {
            if (!this.subscriber.TryGetTarget(out var target))
            {
                return false;
            }

            // TODO: report null handler and failed handlers to diagnostic source
            var handler = this.handlerSelector(target);
            await handler(eventArgs);
            return true;
        }
    }
}