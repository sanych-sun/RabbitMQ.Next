using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class DeleteOkMethodParser : IMethodParser<DeleteOkMethod>
    {
        public DeleteOkMethod Parse(ReadOnlyMemory<byte> payload)
        {
            payload.Read(out uint messageCount);

            return new DeleteOkMethod(messageCount);
        }

        public IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload) => this.Parse(payload);
    }
}