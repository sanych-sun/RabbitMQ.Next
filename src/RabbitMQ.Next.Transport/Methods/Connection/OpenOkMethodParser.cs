using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class OpenOkMethodParser : IMethodParser<OpenOkMethod>
    {
        public OpenOkMethod Parse(ReadOnlySpan<byte> payload) => new OpenOkMethod();

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}