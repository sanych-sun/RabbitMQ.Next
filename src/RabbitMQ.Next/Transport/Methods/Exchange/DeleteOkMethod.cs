using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeleteOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.ExchangeDeleteOk;
    }
}