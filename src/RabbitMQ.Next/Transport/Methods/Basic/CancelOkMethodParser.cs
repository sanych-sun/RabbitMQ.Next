using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class CancelOkMethodParser : IMethodParser<CancelOkMethod>
    {
        public CancelOkMethod Parse(ReadOnlyMemory<byte> payload)
        {
            payload.Read(out string consumerTag);

            return new CancelOkMethod(consumerTag);
        }

        public IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload)
            => this.Parse(payload);
    }
}