using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
{
    public int Write(Span<byte> destination, DeclareMethod method)
    {
        var result = destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(BitConverter.ComposeFlags(method.Passive, method.Durable, method.Exclusive, method.AutoDelete))
            .Write(method.Arguments);

        return destination.Length - result.Length;
    }
}