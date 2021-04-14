using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    internal class ReturnedMessageDelegateHandler : IReturnedMessageHandler
    {
        private Func<ReturnedMessage, IMessageProperties, Content, bool> wrapped;

        public ReturnedMessageDelegateHandler(Func<ReturnedMessage, IMessageProperties, Content, bool> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            this.wrapped = handler;
        }

        public void Dispose()
        {
            this.wrapped = null;
        }

        public bool TryHandle(ReturnedMessage message, IMessageProperties properties, Content content)
        {
            if (this.wrapped == null)
            {
                return false;
            }

            return this.wrapped(message, properties, content);
        }
    }
}