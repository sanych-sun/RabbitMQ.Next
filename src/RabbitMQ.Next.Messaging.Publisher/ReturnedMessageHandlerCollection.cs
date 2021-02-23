using System;
using System.Collections.Generic;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class ReturnedMessageHandlerCollection
    {
        private readonly object sync = new object();
        private readonly List<ReturnedMessageHandlerWrapper> items = new List<ReturnedMessageHandlerWrapper>();

        public IDisposable Add(Action<IReturnedMessage, IContent> handler)
        {
            var wrapped = new ReturnedMessageHandlerWrapper(handler);

            lock (this.sync)
            {
                this.items.Add(wrapped);
            }

            return wrapped;
        }

        public void Invoke(IReturnedMessage message, IContent content)
        {
            lock (this.sync)
            {
                for (int i = this.items.Count - 1; i >= 0; i--)
                {
                    if (!this.items[i].Invoke(message, content))
                    {
                        this.items.RemoveAt(i);
                    }
                }
            }
        }
    }
}