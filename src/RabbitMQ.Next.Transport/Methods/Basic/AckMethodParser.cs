using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class AckMethodParser : IMethodParser<AckMethod>
    {
        public AckMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload
                .Read(out ulong deliveryTag)
                .Read(out bool multiple);

            return new AckMethod(deliveryTag, multiple);
        }

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}