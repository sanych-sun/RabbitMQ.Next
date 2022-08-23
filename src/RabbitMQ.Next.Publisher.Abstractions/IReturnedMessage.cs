using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

public interface IReturnedMessage
{
    public string Exchange { get; }

    public string RoutingKey { get; }

    public ushort ReplyCode { get; }

    public string ReplyText { get; }
    
    IMessageProperties Properties { get; }

    T Content<T>();
}