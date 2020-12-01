using System;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class AckMethodFormatter : IMethodFormatter<AckMethod>
    {
        public Span<byte> Write(Span<byte> destination, AckMethod method)
            => destination
                .Write(method.DeliveryTag)
                .Write(method.Multiple);
    }
}