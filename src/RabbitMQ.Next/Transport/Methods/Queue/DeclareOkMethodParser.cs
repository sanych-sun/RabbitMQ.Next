using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class DeclareOkMethodParser : IMethodParser<DeclareOkMethod>
{
    public DeclareOkMethod Parse(ReadOnlySpan<byte> payload)
    {
        payload
            .Read(out string queue)
            .Read(out uint messageCount)
            .Read(out uint consumerCount);

        return new DeclareOkMethod(queue, messageCount, consumerCount);
    }
}