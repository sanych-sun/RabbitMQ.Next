using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public interface IDeliveredMessage
{
    public string Exchange { get; }

    public string RoutingKey { get; }
    
    public bool Redelivered { get; }
    
    IMessageProperties Properties { get; }

    T Content<T>();
}