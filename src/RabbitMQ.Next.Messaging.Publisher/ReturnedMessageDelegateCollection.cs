using System;
using System.Collections.Generic;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class ReturnedMessageDelegateCollection
    {
        private readonly object sync = new object();
        private readonly List<ReturnedMessageDelegateWrapper> items = new List<ReturnedMessageDelegateWrapper>();

        public IDisposable Add(ReturnedMessageDelegate @delegate)
        {
            var wrapped = new ReturnedMessageDelegateWrapper(@delegate);

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