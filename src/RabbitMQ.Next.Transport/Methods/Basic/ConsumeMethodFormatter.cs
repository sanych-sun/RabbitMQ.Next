using System;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class ConsumeMethodFormatter : IMethodFormatter<ConsumeMethod>
    {
        public Span<byte> Write(Span<byte> destination, ConsumeMethod method)
            => destination
                .Write((short)ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(method.ConsumerTag)
                .Write(method.Flags)
                .Write(method.Arguments);
    }
}