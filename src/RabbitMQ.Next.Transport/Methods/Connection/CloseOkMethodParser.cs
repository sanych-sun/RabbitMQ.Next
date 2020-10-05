using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class CloseOkMethodParser : IMethodParser<CloseOkMethod>
    {
        public CloseOkMethod Parse(ReadOnlySpan<byte> payload)
        {
            return new CloseOkMethod();
        }

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}