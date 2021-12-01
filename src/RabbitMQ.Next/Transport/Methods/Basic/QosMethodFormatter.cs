using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class QosMethodFormatter : IMethodFormatter<QosMethod>
    {
        public int Write(Span<byte> destination, QosMethod method)
        {
            var result = destination
                .Write(method.PrefetchSize)
                .Write(method.PrefetchCount)
                .Write(method.Global);

            return destination.Length - result.Length;
        }
    }
}