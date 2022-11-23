using System;

namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class PurgeMethodFormatter : IMethodFormatter<PurgeMethod>
{
    public Span<byte> Write(Span<byte> destination, PurgeMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(false);
}