using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
    {
        public int Write(Span<byte> destination, DeclareMethod method)
        {
            var result = destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Exchange)
                .Write(method.Type)
                .Write(BitConverter.ComposeFlags(method.Passive, method.Durable, method.AutoDelete, method.Internal))
                .Write(method.Arguments);

            return destination.Length - result.Length;
        }
    }
}