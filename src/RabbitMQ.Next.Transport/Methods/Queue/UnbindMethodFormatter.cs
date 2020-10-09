using System;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
    {
        public Span<byte> Write(Span<byte> destination, UnbindMethod method)
            => destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(method.Exchange)
                .Write(method.RoutingKey)
                .Write(method.Arguments);
    }
}