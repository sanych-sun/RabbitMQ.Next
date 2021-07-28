using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class PurgeMethodFormatter : IMethodFormatter<PurgeMethod>
    {
        public int Write(Memory<byte> destination, PurgeMethod method)
        {
            var result = destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(false);

            return destination.Length - result.Length;
        }
    }
}