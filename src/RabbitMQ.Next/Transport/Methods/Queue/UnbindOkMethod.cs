using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct UnbindOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.QueueUnbindOk;
    }
}