using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods;

internal class EmptyArgsParser<TMethod> : IMethodParser<TMethod>
    where TMethod: struct, IIncomingMethod
{
    public TMethod Parse(ReadOnlySpan<byte> payload)
        => default;
}