using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public class MessageHeader
    {
        public string Exchange { get; set; }

        public string RoutingKey { get; set; }

        public MessageProperties Properties { get; set; }

        public bool? Mandatory { get; set; }

        public bool? Immediate { get; set; }
    }
}