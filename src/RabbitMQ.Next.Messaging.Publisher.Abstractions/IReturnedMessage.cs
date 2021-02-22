using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IReturnedMessage
    {
        string Exchange { get; }

        string RoutingKey { get; }

        IMessageProperties Properties { get; }

        ushort ReplyCode { get; }

        string ReplyText { get; }
    }
}