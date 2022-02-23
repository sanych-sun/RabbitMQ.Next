using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    internal class BindMethodFormatter : IMethodFormatter<BindMethod>
    {
        public int Write(Span<byte> destination, BindMethod method)
        {
            var result = destination.Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Destination)
                .Write(method.Source)
                .Write(method.RoutingKey)
                .Write(false)
                .Write(method.Arguments);

            return destination.Length - result.Length;
        }
    }
}