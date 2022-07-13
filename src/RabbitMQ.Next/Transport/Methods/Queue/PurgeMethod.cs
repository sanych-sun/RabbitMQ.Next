using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue;

public readonly struct PurgeMethod : IOutgoingMethod
{
    public PurgeMethod(string queue)
    {
        this.Queue = queue;
    }

    public MethodId MethodId => MethodId.QueuePurge;

    public string Queue { get; }
}