using System;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class CancelMethodFormatter : IMethodFormatter<CancelMethod>
{
    public Span<byte> Write(Span<byte> destination, CancelMethod method)
        => destination
            .Write(method.ConsumerTag)
            .Write(false); // noWait flag
}