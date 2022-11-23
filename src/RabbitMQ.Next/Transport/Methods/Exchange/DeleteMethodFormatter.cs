using System;

namespace RabbitMQ.Next.Transport.Methods.Exchange;

public class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
{
    public Span<byte> Write(Span<byte> destination, DeleteMethod method)
        => destination.Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Exchange)
            .Write(method.UnusedOnly);
}