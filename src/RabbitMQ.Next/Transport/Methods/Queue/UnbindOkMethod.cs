using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct UnbindOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.QueueUnbindOk;
    }
}