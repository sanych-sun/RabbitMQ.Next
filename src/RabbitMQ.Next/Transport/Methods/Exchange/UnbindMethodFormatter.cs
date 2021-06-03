using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
    {
        public Span<byte> Write(Span<byte> destination, UnbindMethod method)
            => destination.Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Destination)
                .Write(method.Source)
                .Write(method.RoutingKey)
                .Write(false)
                .Write(method.Arguments);
        }
}