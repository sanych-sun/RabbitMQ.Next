using System;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
    {
        public Span<byte> Write(Span<byte> destination, DeclareMethod method)
        {
            var flags = Convert.ToInt32(method.Passive) << 0;

            flags |= (byte)method.Flags;

            return destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write((byte) flags)
                .Write(method.Arguments);
        }
    }
}