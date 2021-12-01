using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
    {
        public int Write(Span<byte> destination, DeleteMethod method)
        {
            var result = destination.Write((short) ProtocolConstants.ObsoleteField)
                .Write(method.Exchange)
                .Write(method.UnusedOnly);

            return destination.Length - result.Length;
        }
    }
}