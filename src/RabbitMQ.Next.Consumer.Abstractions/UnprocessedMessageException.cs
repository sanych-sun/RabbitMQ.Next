using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public class UnprocessedMessageException : Exception
    {
        public UnprocessedMessageException(DeliveredMessage message, IMessageProperties properties, Content content)
            : base("No handler found for the delivered message.")
        {
            this.DeliveredMessage = message;
            this.Properties = properties;
            this.Content = content;
        }

        public DeliveredMessage DeliveredMessage { get; }

        public IMessageProperties Properties { get; }

        public Content Content { get; }
    }
}