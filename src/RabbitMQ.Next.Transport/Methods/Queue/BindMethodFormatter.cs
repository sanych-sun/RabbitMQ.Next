using System;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class BindMethodFormatter : IMethodFormatter<BindMethod>
    {
        public Span<byte> Write(Span<byte> destination, BindMethod method)
            => destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(method.Exchange)
                .Write(method.RoutingKey)
                .Write(false)
                .Write(method.Arguments);
    }
}