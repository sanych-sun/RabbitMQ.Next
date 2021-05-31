using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Events;

namespace RabbitMQ.Next.Transport.Events
{
    internal class EventSource<TEventArgs> : IEventSource<TEventArgs>
    {
        private readonly List<ISubscription<TEventArgs>> subscriptions = new List<ISubscription<TEventArgs>>();
        private readonly SemaphoreSlim sync = new SemaphoreSlim(1, 1);

        public IDisposable Subscribe<TSubscriber>(TSubscriber subscriber, Func<TSubscriber, Func<TEventArgs, ValueTask>> handlerSelector)
            where TSubscriber : class
        {
            var subscription = new Subscription<TSubscriber, TEventArgs>(subscriber, handlerSelector);
            this.AddSubscription(subscription).GetAwaiter().GetResult();
            return subscription;
        }

        public bool HasSubscribers() => this.subscriptions.Count > 0;

        public async ValueTask InvokeAsync(TEventArgs eventArgs)
        {
            if (this.subscriptions.Count == 0)
            {
                return;
            }

            await this.sync.WaitAsync();
            try
            {
                for (var i = this.subscriptions.Count - 1; i >= 0; i--)
                {
                    var result = true;
                    try
                    {
                        result = await this.subscriptions[i].HandleAsync(eventArgs);
                    }
                    catch (Exception e)
                    {
                        // TODO: report failed subscriber to diagnostic source
                    }

                    if (!result)
                    {
                        this.subscriptions.RemoveAt(i);
                    }
                }
            }
            finally
            {
                this.sync.Release();
            }
        }

        internal async Task AddSubscription(ISubscription<TEventArgs> subscription)
        {
            await this.sync.WaitAsync();
            try
            {
                this.subscriptions.Add(subscription);
            }
            finally
            {
                this.sync.Release();
            }
        }
    }
}