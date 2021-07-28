using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class DeclareOkMethodParser : IMethodParser<DeclareOkMethod>
    {
        public DeclareOkMethod Parse(ReadOnlyMemory<byte> payload)
        {
            payload
                .Read(out string queue)
                .Read(out uint messageCount)
                .Read(out uint consumerCount);

            return new DeclareOkMethod(queue, messageCount, consumerCount);
        }

        public IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload) => this.Parse(payload);
    }
}