using System;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class GetOkMethodParser : IMethodParser<GetOkMethod>
{
    public GetOkMethod Parse(ReadOnlySpan<byte> payload)
    {
        payload
            .Read(out ulong deliveryTag)
            .Read(out bool redelivered)
            .Read(out string exchange)
            .Read(out string routingKey)
            .Read(out uint messageCount);

        return new GetOkMethod(exchange, routingKey, deliveryTag, redelivered, messageCount);
    }
}