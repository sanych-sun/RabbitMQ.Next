using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public interface IDeliveredMessage : IMessageProperties
{
    public string Exchange { get; }

    public string RoutingKey { get; }
    
    public bool Redelivered { get; }
}