using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class ConsumeMethodFormatter : IMethodFormatter<ConsumeMethod>
    {
        public int Write(Span<byte> destination, ConsumeMethod method)
        {
            var result = destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(method.ConsumerTag)
                .Write(method.Flags)
                .Write(method.Arguments);

            return destination.Length - result.Length;
        }
    }
}