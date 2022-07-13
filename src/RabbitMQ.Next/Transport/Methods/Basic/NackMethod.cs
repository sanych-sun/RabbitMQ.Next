using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct NackMethod : IOutgoingMethod, IIncomingMethod
{
    public NackMethod(ulong deliveryTag, bool multiple, bool requeue)
    {
        this.DeliveryTag = deliveryTag;
        this.Multiple = multiple;
        this.Requeue = requeue;
    }

    public MethodId MethodId => MethodId.BasicNack;

    public ulong DeliveryTag { get; }

    public bool Multiple { get; }

    public bool Requeue { get; }
}