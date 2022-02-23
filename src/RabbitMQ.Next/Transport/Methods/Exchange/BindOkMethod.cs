using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct BindOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.ExchangeBindOk;
    }
}