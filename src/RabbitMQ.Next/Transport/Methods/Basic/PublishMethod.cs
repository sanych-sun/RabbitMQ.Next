using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct PublishMethod : IOutgoingMethod
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

    public MethodId MethodId => MethodId.BasicPublish;

    public string Exchange { get; }

    public string RoutingKey { get; }

    public byte Flags { get; }
}