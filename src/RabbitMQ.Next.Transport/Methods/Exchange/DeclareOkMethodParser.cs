using System;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class DeclareOkMethodParser : IMethodParser<DeclareOkMethod>
    {
        public DeclareOkMethod Parse(ReadOnlySpan<byte> payload)
            => new DeclareOkMethod();

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}