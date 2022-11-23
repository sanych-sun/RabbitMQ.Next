using System;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class NackMethodFormatter : IMethodFormatter<NackMethod>
{
    public Span<byte> Write(Span<byte> destination, NackMethod method)
        => destination
            .Write(method.DeliveryTag)
            .Write(BitConverter.ComposeFlags(method.Multiple, method.Requeue));
}