using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    internal class FlowMethodFormatter : IMethodFormatter<FlowMethod>
    {
        public int Write(Memory<byte> destination, FlowMethod method)
        {
            var result = destination.Write(method.Active);

            return destination.Length - result.Length;
        }
    }
}