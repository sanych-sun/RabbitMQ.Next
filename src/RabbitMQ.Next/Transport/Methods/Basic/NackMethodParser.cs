
using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class NackMethodParser : IMethodParser<NackMethod>
    {
        public NackMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload
                .Read(out ulong deliveryTag)
                .Read(out byte flags);

            return new NackMethod(deliveryTag, BitConverter.IsFlagSet(flags, 0), BitConverter.IsFlagSet(flags, 1));
        }
    }
}