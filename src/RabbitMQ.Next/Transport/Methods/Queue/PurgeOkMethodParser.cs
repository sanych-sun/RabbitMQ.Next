using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class PurgeOkMethodParser : IMethodParser<PurgeOkMethod>
    {
        public PurgeOkMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out uint messageCount);

            return new PurgeOkMethod(messageCount);
        }
    }
}