using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class GetMethodFormatter : IMethodFormatter<GetMethod>
{
    public int Write(Span<byte> destination, GetMethod method)
    {
        var result = destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(method.NoAck);

        return destination.Length - result.Length;
    }
}