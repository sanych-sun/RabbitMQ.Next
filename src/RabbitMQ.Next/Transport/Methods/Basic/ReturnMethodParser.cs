using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class ReturnMethodParser : IMethodParser<ReturnMethod>
{
    public ReturnMethod Parse(ReadOnlySpan<byte> payload)
    {
        payload
            .Read(out ushort replyCode)
            .Read(out string replyText)
            .Read(out string exchange)
            .Read(out string routingKey);

        return new ReturnMethod(exchange, routingKey, replyCode, replyText);
    }
}