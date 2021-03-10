using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeleteOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.ExchangeDeleteOk;
    }
}