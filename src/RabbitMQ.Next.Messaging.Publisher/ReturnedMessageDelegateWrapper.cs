using System;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class ReturnedMessageDelegateWrapper : IDisposable
    {
        private ReturnedMessageDelegate item;

        public ReturnedMessageDelegateWrapper(ReturnedMessageDelegate item)
        {
            this.item = item;
        }

        public bool Invoke(IReturnedMessage message, IContent content)
        {
            var c = this.item;
            if (c == null)
            {
                return false;
            }

            c(message, content);
            return true;
        }

        public void Dispose() => this.item = null;
    }
}