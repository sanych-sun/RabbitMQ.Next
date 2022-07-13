using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct AckMethod : IOutgoingMethod, IIncomingMethod
{
    public AckMethod(ulong deliveryTag, bool multiple)
    {
        this.DeliveryTag = deliveryTag;
        this.Multiple = multiple;
    }

    public MethodId MethodId => MethodId.BasicAck;

    public ulong DeliveryTag { get; }

    public bool Multiple { get; }
}