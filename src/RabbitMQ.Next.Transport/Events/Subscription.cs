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

            var handler = this.handlerSelector(target);
            // TODO: report null handler to diagnostic source
            if (handler == null)
            {
                // TODO: should it be false instead?
                return true;
            }

            await handler(eventArgs);
            return true;
        }
    }
}