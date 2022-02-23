using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct UnbindOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.ExchangeUnbindOk;
    }
}