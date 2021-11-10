using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    internal class DeliveredMessageDelegateHandler : IDeliveredMessageHandler
    {
        private Func<DeliveredMessage, IMessageProperties, IContentAccessor, ValueTask<bool>> wrapped;

        public DeliveredMessageDelegateHandler(Func<DeliveredMessage, IMessageProperties, IContentAccessor, ValueTask<bool>> handler)
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

        public ValueTask<bool> TryHandleAsync(DeliveredMessage message, IMessageProperties properties, IContentAccessor content)
        {
            if (this.wrapped == null)
            {
                return new ValueTask<bool>(false);
            }

            return this.wrapped(message, properties, content);
        }
    }
}