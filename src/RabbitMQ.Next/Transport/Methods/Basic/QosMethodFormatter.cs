using System;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class QosMethodFormatter : IMethodFormatter<QosMethod>
{
    public Span<byte> Write(Span<byte> destination, QosMethod method)
        => destination
            .Write(method.PrefetchSize)
            .Write(method.PrefetchCount)
            .Write(method.Global);
}