using System;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public class UnprocessedMessageException : Exception
{
    public UnprocessedMessageException(DeliveredMessage message, IContent content)
        : base("No handler found for the delivered message.")
    {
        this.DeliveredMessage = message;
        this.Content = content;
    }

    public DeliveredMessage DeliveredMessage { get; }

    public IContent Content { get; }
}