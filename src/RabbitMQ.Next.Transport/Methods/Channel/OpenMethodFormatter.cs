using System;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class OpenMethodFormatter : IMethodFormatter<OpenMethod>
    {
        public Span<byte> Write(Span<byte> destination, OpenMethod method)
            => destination.Write(ProtocolConstants.ObsoleteFieldByte);
    }
}