using System;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class DeleteOkMethodParser : IMethodParser<DeleteOkMethod>
    {
        public DeleteOkMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out uint messageCount);

            return new DeleteOkMethod(messageCount);
        }

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}