using System;
using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class ReturnedMessageHandlerWrapper : IDisposable
    {
        private Action<IReturnedMessage, IContent> handler;

        public ReturnedMessageHandlerWrapper(Action<IReturnedMessage, IContent> handler)
        {
            this.handler = handler;
        }

        public bool Invoke(IReturnedMessage message, IContent content)
        {
            var c = this.handler;
            if (c == null)
            {
                return false;
            }

            c(message, content);
            return true;
        }

        public void Dispose() => this.handler = null;
    }
}