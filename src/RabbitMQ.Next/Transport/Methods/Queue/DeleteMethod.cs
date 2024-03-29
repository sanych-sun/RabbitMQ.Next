using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue;

public readonly struct DeleteMethod : IOutgoingMethod
{
    public DeleteMethod(string queue, bool unusedOnly, bool emptyOnly)
    {
        this.Queue = queue;
        this.UnusedOnly = unusedOnly;
        this.EmptyOnly = emptyOnly;
    }

    public MethodId MethodId => MethodId.QueueDelete;

    public string Queue { get; }

    public bool UnusedOnly { get; }

    public bool EmptyOnly { get; }
}