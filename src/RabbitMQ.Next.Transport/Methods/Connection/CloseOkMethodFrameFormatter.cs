using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public class CloseOkMethodFrameFormatter : IMethodFrameFormatter<CloseOkMethod>
    {
        public Span<byte> Write(Span<byte> destination, CloseOkMethod method)
        {
            return destination;
        }
    }
}