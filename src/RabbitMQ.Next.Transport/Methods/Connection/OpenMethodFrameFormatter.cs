using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class OpenMethodFrameFormatter : IMethodFrameFormatter<OpenMethod>
    {
        public Span<byte> Write(Span<byte> destination, OpenMethod method) =>
            destination
                .Write(method.VirtualHost)
                .Write(ProtocolConstants.ObsoleteFieldByte)
                .Write(ProtocolConstants.ObsoleteFieldByte);
    }
}