using System;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class DeliverMethodParser : IMethodParser<DeliverMethod>
{
    public DeliverMethod Parse(ReadOnlySpan<byte> payload)
    {
        payload
            .Read(out string consumerTag)
            .Read(out ulong deliveryTag)
            .Read(out bool redelivered)
            .Read(out string exchange)
            .Read(out string routingKey);

        return new DeliverMethod(exchange, routingKey, consumerTag, deliveryTag, redelivered);
    }
}