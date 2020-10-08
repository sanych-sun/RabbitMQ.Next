using System;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class OpenOkMethodParser : IMethodParser<OpenOkMethod>
    {
        public OpenOkMethod Parse(ReadOnlySpan<byte> payload) => new OpenOkMethod();

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}