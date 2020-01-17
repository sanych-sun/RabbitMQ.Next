using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class OpenOkMethodFrameParser : IMethodFrameParser<OpenOkMethod>
    {
        public OpenOkMethod Parse(ReadOnlySpan<byte> payload) => new OpenOkMethod();

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}