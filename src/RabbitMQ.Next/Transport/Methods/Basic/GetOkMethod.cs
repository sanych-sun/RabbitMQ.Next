using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct GetOkMethod : IIncomingMethod, IHasContentMethod
{
    public GetOkMethod(string exchange, string routingKey, ulong deliveryTag, bool redelivered, uint messageCount)
    {
        this.Exchange = exchange;
        this.RoutingKey = routingKey;
        this.DeliveryTag = deliveryTag;
        this.Redelivered = redelivered;
        this.MessageCount = messageCount;
    }

    public MethodId MethodId => MethodId.BasicGetOk;

    public string Exchange { get; }

    public string RoutingKey { get; }

    public ulong DeliveryTag { get; }

    public bool Redelivered { get; }

    public uint MessageCount { get; }
}