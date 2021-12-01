using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class FlowOkMethodFormatter : IMethodFormatter<FlowOkMethod>
    {
        public int Write(Span<byte> destination, FlowOkMethod method)
        {
            var result = destination.Write(method.Active);

            return destination.Length - result.Length;
        }
    }
}