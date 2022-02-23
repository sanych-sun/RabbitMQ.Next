using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
    {
        public int Write(Span<byte> destination, UnbindMethod method)
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