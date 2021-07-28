using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class ReturnMethodParser : IMethodParser<ReturnMethod>
    {
        public ReturnMethod Parse(ReadOnlyMemory<byte> payload)
        {
            payload
                .Read(out ushort replyCode)
                .Read(out string replyText)
                .Read(out string exchange)
                .Read(out string routingKey);

            return new ReturnMethod(exchange, routingKey, replyCode, replyText);
        }

        public IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload) => this.Parse(payload);
    }
}