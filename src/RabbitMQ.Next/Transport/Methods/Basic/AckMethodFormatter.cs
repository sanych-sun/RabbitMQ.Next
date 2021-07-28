using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class AckMethodFormatter : IMethodFormatter<AckMethod>
    {
        public int Write(Memory<byte> destination, AckMethod method)
        {
            var result = destination
                .Write(method.DeliveryTag)
                .Write(method.Multiple);

            return destination.Length - result.Length;
        }
    }
}