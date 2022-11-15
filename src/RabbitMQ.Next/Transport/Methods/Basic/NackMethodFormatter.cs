using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class NackMethodFormatter : IMethodFormatter<NackMethod>
{
    public int Write(Span<byte> destination, NackMethod method)
    {
        var result = destination
            .Write(method.DeliveryTag)
            .Write(BitConverter.ComposeFlags(method.Multiple, method.Requeue));

        return destination.Length - result.Length;
    }
}