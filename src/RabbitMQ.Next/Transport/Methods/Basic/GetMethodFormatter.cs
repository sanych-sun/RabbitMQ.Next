using System;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class GetMethodFormatter : IMethodFormatter<GetMethod>
{
    public Span<byte> Write(Span<byte> destination, GetMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(method.NoAck);
}