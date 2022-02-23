using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class FlowOkMethodParser : IMethodParser<FlowOkMethod>
    {
        public FlowOkMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out bool active);

            return new FlowOkMethod(active);
        }
    }
}