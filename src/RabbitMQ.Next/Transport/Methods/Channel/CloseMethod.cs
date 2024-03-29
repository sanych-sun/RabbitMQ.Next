using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel;

public readonly struct CloseMethod : IIncomingMethod, IOutgoingMethod
{
    public CloseMethod(ushort statusCode, string description, MethodId failedMethod)
    {
        this.StatusCode = statusCode;
        this.Description = description;
        this.FailedMethodId = failedMethod;
    }

    public MethodId MethodId => MethodId.ChannelClose;

    public ushort StatusCode { get; }

    public string Description { get; }

    public MethodId FailedMethodId { get; }
}