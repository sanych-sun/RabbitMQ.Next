using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer
{
    internal class DeliveredMessageDelegateHandler : IDeliveredMessageHandler
    {
        private Func<DeliveredMessage, IContent, ValueTask<bool>> wrapped;

        public DeliveredMessageDelegateHandler(Func<DeliveredMessage, IContent, ValueTask<bool>> handler)
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

        public ValueTask<bool> TryHandleAsync(DeliveredMessage message, IContent content)
        {
            if (this.wrapped == null)
            {
                throw new ObjectDisposedException(nameof(DeliveredMessageDelegateHandler));
            }

            return this.wrapped(message, content);
        }
    }
}