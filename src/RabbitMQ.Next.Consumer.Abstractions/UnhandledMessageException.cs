using System;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public class UnhandledMessageException : Exception
    {
        public UnhandledMessageException(IDeliveredMessage message, IContent content)
            : base("No handler found for the delivered message.")
        {
            this.DeliveredMessage = message;
            this.Content = content;
        }

        public IDeliveredMessage DeliveredMessage { get; }

        public IContent Content { get; }
    }
}