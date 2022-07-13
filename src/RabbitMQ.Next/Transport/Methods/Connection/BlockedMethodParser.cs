using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class BlockedMethodParser : IMethodParser<BlockedMethod>
{
    public BlockedMethod Parse(ReadOnlySpan<byte> payload)
    {
        payload.Read(out string reason);

        return new BlockedMethod(reason);
    }
}