using System;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class DeleteOkMethodParser : IMethodParser<DeleteOkMethod>
    {
        public DeleteOkMethod Parse(ReadOnlySpan<byte> payload)
            => new DeleteOkMethod();

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}