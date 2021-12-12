using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    internal class DeliveredMessageDelegateHandler : IDeliveredMessageHandler
    {
        private Func<DeliveredMessage, IContentAccessor, ValueTask<bool>> wrapped;

        public DeliveredMessageDelegateHandler(Func<DeliveredMessage, IContentAccessor, ValueTask<bool>> handler)
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

        public ValueTask<bool> TryHandleAsync(DeliveredMessage message, IContentAccessor content)
        {
            if (this.wrapped == null)
            {
                throw new ObjectDisposedException(nameof(DeliveredMessageDelegateHandler));
            }

            return this.wrapped(message, content);
        }
    }
}