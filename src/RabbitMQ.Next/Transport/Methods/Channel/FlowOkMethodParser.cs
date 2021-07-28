using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class FlowOkMethodParser : IMethodParser<FlowOkMethod>
    {
        public FlowOkMethod Parse(ReadOnlyMemory<byte> payload)
        {
            payload.Read(out bool active);

            return new FlowOkMethod(active);
        }

        public IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload) => this.Parse(payload);
    }
}