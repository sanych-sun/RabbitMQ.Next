using System;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer
{
    public class UnprocessedMessageException : Exception
    {
        public UnprocessedMessageException(DeliveredMessage message, IMessageProperties properties, IContentAccessor content)
            : base("No handler found for the delivered message.")
        {
            this.DeliveredMessage = message;
            this.Properties = properties;
            this.Content = content;
        }

        public DeliveredMessage DeliveredMessage { get; }

        public IMessageProperties Properties { get; }

        public IContentAccessor Content { get; }
    }
}