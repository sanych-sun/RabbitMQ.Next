using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct OpenOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.ConnectionOpenOk;
    }
}