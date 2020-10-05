using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public class CloseOkMethodFrameParser : IMethodFrameParser<CloseOkMethod>
    {
        public CloseOkMethod Parse(ReadOnlySpan<byte> payload)
        {
            return new CloseOkMethod();
        }

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}