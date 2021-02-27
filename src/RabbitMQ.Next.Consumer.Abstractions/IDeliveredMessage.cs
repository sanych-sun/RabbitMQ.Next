using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IDeliveredMessage
    {
        string Exchange { get; }

        string RoutingKey { get; }

        IMessageProperties Properties { get; }

        bool Redelivered { get; }

        string ConsumerTag { get; }

        ulong DeliveryTag { get; }
    }
}