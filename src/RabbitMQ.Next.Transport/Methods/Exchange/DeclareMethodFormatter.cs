using System;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
    {
        public Span<byte> Write(Span<byte> destination, DeclareMethod method)
        {
            int flags = (Convert.ToInt32(method.Passive) << 0);

            flags |= (byte)method.Flags;

            return destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Exchange)
                .Write(method.Type)
                .Write((byte) flags)
                .Write(method.Arguments);
        }
    }
}