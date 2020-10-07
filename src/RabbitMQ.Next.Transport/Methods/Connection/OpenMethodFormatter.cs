using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class OpenMethodFormatter : IMethodFormatter<OpenMethod>
    {
        public Span<byte> Write(Span<byte> destination, OpenMethod method) =>
            destination
                .Write(method.VirtualHost)
                .Write(ProtocolConstants.ObsoleteField)
                .Write(ProtocolConstants.ObsoleteField);
    }
}