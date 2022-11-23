using System;

namespace RabbitMQ.Next.Transport.Methods.Exchange;

internal class BindMethodFormatter : IMethodFormatter<BindMethod>
{
    public Span<byte> Write(Span<byte> destination, BindMethod method)
        => destination.Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Destination)
            .Write(method.Source)
            .Write(method.RoutingKey)
            .Write(false)
            .Write(method.Arguments);
}