using System;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class BindOkMethodParser : IMethodParser<BindOkMethod>
    {
        public BindOkMethod Parse(ReadOnlySpan<byte> payload)
            => new BindOkMethod();

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}