using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal readonly struct PublishMethod : IOutgoingMethod
    {
        public PublishMethod(string exchange, string routingKey, bool mandatory, bool immediate)
            : this(exchange, routingKey, BitConverter.ComposeFlags(mandatory, immediate))
        {
        }

        public PublishMethod(string exchange, string routingKey, byte flags)
        {
            this.Exchange = exchange;
            this.RoutingKey = routingKey;
            this.Flags = flags;
        }

        public uint Method => (uint) MethodId.BasicPublish;

        public string Exchange { get; }

        public string RoutingKey { get; }

        public byte Flags { get; }
    }
}