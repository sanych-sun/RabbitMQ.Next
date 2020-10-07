using System;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class UnbindOkMethodParser : IMethodParser<UnbindOkMethod>
    {
        public UnbindOkMethod Parse(ReadOnlySpan<byte> payload)
            => new UnbindOkMethod();

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}