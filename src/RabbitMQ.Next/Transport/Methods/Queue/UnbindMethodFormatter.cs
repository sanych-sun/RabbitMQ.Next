using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    internal class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
    {
        public int Write(Memory<byte> destination, UnbindMethod method)
        {
            var result = destination
                .Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Queue)
                .Write(method.Exchange)
                .Write(method.RoutingKey)
                .Write(method.Arguments);

            return destination.Length - result.Length;
        }
    }
}