using System;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
    {
        public Span<byte> Write(Span<byte> destination, DeclareMethod method)
            => destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write((byte) method.Flags)
                .Write(method.Arguments);
    }
}