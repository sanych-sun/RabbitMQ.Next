using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class FlowMethodParser : IMethodParser<FlowMethod>
    {
        public FlowMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out bool active);

            return new FlowMethod(active);
        }
    }
}