using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class CancelOkMethodParser : IMethodParser<CancelOkMethod>
    {
        public CancelOkMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out string consumerTag);

            return new CancelOkMethod(consumerTag);
        }
    }
}