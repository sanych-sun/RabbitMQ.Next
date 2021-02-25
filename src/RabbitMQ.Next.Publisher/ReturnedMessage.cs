using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal class ReturnedMessage : IReturnedMessage
    {
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }

        public IMessageProperties Properties { get; set; }
        public ushort ReplyCode { get; set; }
        public string ReplyText { get; set; }
    }
}