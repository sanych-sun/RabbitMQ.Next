using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer.Abstractions;

namespace RabbitMQ.Next.Consumer
{
    internal class DeliveredMessage: IDeliveredMessage
    {
        public string Exchange { get; set; }

        public string RoutingKey { get; set; }

        public IMessageProperties Properties { get; set; }

        public bool Redelivered { get; set; }

        public string ConsumerTag { get; set; }

        public ulong DeliveryTag { get; set; }
    }
}