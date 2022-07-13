using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct QosMethod : IOutgoingMethod
{
    public QosMethod(uint prefetchSize, ushort prefetchCount, bool global)
    {
        this.PrefetchSize = prefetchSize;
        this.PrefetchCount = prefetchCount;
        this.Global = global;
    }

    public MethodId MethodId => MethodId.BasicQos;

    public uint PrefetchSize { get; }

    public ushort PrefetchCount { get; }

    public bool Global { get; }
}