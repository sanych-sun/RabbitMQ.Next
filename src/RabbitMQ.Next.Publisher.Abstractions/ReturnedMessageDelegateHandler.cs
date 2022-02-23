using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher
{
    internal class ReturnedMessageDelegateHandler : IReturnedMessageHandler
    {
        private Func<ReturnedMessage, IContentAccessor, ValueTask<bool>> wrapped;

        public ReturnedMessageDelegateHandler(Func<ReturnedMessage, IContentAccessor, ValueTask<bool>> handler)
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

        public ValueTask<bool> TryHandleAsync(ReturnedMessage message, IContentAccessor content)
        {
            if (this.wrapped == null)
            {
                return new ValueTask<bool>(false);
            }

            return this.wrapped(message, content);
        }
    }
}