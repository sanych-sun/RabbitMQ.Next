using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class PublishMethodFormatter : IMethodFormatter<PublishMethod>
{
    public int Write(Span<byte> destination, PublishMethod method)
    {
        var result = destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Exchange)
            .Write(method.RoutingKey)
            .Write(method.Flags);

        return destination.Length - result.Length;
    }
}