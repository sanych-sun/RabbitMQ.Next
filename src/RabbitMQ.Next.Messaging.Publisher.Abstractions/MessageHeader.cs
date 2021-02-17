using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public class MessageHeader
    {
        public MessageHeader()
        {
            this.Properties = new MessageProperties();
        }

        public string Exchange { get; set; }

        public string RoutingKey { get; set; }

        public MessageProperties Properties { get; }

        public bool? Mandatory { get; set; }

        public bool? Immediate { get; set; }
    }
}