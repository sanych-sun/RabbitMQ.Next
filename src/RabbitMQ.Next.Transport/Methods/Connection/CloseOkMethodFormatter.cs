using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class CloseOkMethodFormatter : IMethodFormatter<CloseOkMethod>
    {
        public Span<byte> Write(Span<byte> destination, CloseOkMethod method)
            => destination;
    }
}