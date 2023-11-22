using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

public interface IReturnedMessage : IMessageProperties
{
    public string Exchange { get; }

    public string RoutingKey { get; }

    public ushort ReplyCode { get; }

    public string ReplyText { get; }
}