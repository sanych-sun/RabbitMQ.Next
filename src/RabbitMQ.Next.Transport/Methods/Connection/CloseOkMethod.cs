using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct CloseOkMethod : IIncomingMethod, IOutgoingMethod
    {
        public MethodId MethodId => MethodId.ConnectionCloseOk;
    }
}