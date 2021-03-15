using System;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    internal class ReturnedMessageDelegateHandler : IReturnedMessageHandler
    {
        private readonly Func<ReturnedMessage, IContent, bool> wrapped;

        public ReturnedMessageDelegateHandler(Func<ReturnedMessage, IContent, bool> handler)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            this.wrapped = handler;
        }

        public void Dispose() { }

        public bool TryHandle(ReturnedMessage message, IContent content) => this.wrapped(message, content);
    }
}