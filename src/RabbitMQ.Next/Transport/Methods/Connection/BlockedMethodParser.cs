using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class BlockedMethodParser : IMethodParser<BlockedMethod>
    {
        public BlockedMethod Parse(ReadOnlyMemory<byte> payload)
        {
            payload.Read(out string reason);

            return new BlockedMethod(reason);
        }

        public IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload) => this.Parse(payload);
    }
}