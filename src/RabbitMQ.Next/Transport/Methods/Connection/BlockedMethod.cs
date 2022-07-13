using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection;

public readonly struct BlockedMethod : IIncomingMethod
{
    public BlockedMethod(string reason)
    {
        this.Reason = reason;
    }

    public MethodId MethodId => MethodId.ConnectionBlocked;

    public string Reason { get; }
}