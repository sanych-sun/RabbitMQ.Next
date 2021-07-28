using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
    {
        public int Write(Memory<byte> destination, DeleteMethod method)
        {
            var result = destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(BitConverter.ComposeFlags(method.UnusedOnly, method.EmptyOnly));

            return destination.Length - result.Length;
        }
    }
}