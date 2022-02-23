using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeclareOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.ExchangeDeclareOk;
    }
}