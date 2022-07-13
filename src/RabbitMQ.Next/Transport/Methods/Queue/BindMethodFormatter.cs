using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class BindMethodFormatter : IMethodFormatter<BindMethod>
{
    public int Write(Span<byte> destination, BindMethod method)
    {
        var result = destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(method.Exchange)
            .Write(method.RoutingKey)
            .Write(false)
            .Write(method.Arguments);

        return destination.Length - result.Length;
    }
}