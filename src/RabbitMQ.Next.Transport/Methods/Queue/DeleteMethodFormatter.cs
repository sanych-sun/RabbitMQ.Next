using System;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
    {
        public Span<byte> Write(Span<byte> destination, DeleteMethod method)
            => destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(BitConverter.ComposeFlags(method.UnusedOnly, method.EmptyOnly));
    }
}