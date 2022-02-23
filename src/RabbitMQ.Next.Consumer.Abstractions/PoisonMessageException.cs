using System;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer
{
    public class PoisonMessageException : Exception
    {
        public PoisonMessageException(DeliveredMessage message, IMessageProperties properties, IContentAccessor content, Exception inner)
            : base("Failed to handle the delivered message", inner)
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