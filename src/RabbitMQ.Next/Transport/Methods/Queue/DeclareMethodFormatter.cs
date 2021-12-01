using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
    {
        public int Write(Span<byte> destination, DeclareMethod method)
        {
            var result = destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(method.Flags)
                .Write(method.Arguments);

            return destination.Length - result.Length;
        }
    }
}