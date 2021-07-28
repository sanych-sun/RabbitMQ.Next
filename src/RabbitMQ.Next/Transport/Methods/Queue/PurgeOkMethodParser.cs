using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class PurgeOkMethodParser : IMethodParser<PurgeOkMethod>
    {
        public PurgeOkMethod Parse(ReadOnlyMemory<byte> payload)
        {
            payload.Read(out uint messageCount);

            return new PurgeOkMethod(messageCount);
        }

        public IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload) => this.Parse(payload);
    }
}