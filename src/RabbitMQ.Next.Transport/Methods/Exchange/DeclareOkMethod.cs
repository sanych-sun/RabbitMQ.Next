using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeclareOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.ExchangeDeclareOk;
    }
}