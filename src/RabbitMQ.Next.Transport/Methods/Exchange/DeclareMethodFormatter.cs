using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
    {
        public Span<byte> Write(Span<byte> destination, DeclareMethod method) =>
            destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Exchange)
                .Write(method.Type)
                .Write(method.Flags)
                .Write(method.Arguments);
    }
}