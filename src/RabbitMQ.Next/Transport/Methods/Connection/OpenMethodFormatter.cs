using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class OpenMethodFormatter : IMethodFormatter<OpenMethod>
{
    public int Write(Span<byte> destination, OpenMethod method)
    {
        var result = destination
            .Write(method.VirtualHost)
            .Write(ProtocolConstants.ObsoleteField)
            .Write(ProtocolConstants.ObsoleteField);

        return destination.Length - result.Length;
    }
}