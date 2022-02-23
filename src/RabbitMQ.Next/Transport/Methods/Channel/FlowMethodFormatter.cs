using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class FlowMethodFormatter : IMethodFormatter<FlowMethod>
    {
        public int Write(Span<byte> destination, FlowMethod method)
        {
            var result = destination.Write(method.Active);

            return destination.Length - result.Length;
        }
    }
}