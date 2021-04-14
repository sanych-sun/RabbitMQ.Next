using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public class PoisonMessageException : Exception
    {
        public PoisonMessageException(DeliveredMessage message, IMessageProperties properties, Content content, Exception inner)
            : base("Failed to handle the delivered message", inner)
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