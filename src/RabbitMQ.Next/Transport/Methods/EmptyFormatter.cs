using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods;

internal class EmptyFormatter<TMethod> : IMethodFormatter<TMethod>
    where TMethod: struct, IOutgoingMethod
{
    public int Write(Span<byte> destination, TMethod method) => 0;
}