using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class CloseOkMethodParser : IMethodParser<CloseOkMethod>
    {
        public CloseOkMethod Parse(ReadOnlySpan<byte> payload) => new CloseOkMethod();

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}