using System;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class CloseOkMethodParser : IMethodParser<CloseOkMethod>
    {
        public CloseOkMethod Parse(ReadOnlySpan<byte> payload)
            => new CloseOkMethod();

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}