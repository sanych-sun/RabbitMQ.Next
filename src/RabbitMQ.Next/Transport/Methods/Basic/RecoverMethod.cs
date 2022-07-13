using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic;

public readonly struct RecoverMethod : IOutgoingMethod
{
    public RecoverMethod(bool requeue)
    {
        this.Requeue = requeue;
    }

    public MethodId MethodId => MethodId.BasicRecover;

    public bool Requeue { get; }
}