using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public class CloseOkMethodFormatter : IMethodFormatter<CloseOkMethod>
    {
        public Span<byte> Write(Span<byte> destination, CloseOkMethod method)
        {
            return destination;
        }
    }
}