using System;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public class UnhandledMessageException : Exception
    {
        public UnhandledMessageException(DeliveredMessage message, IContent content)
            : base("No handler found for the delivered message.")
        {
            this.DeliveredMessage = message;
            this.Content = content;
        }

        public DeliveredMessage DeliveredMessage { get; }

        public IContent Content { get; }
    }
}