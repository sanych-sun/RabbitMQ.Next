using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel;

public readonly struct FlowOkMethod : IIncomingMethod, IOutgoingMethod
{
    public FlowOkMethod(bool active)
    {
        this.Active = active;
    }

    public MethodId MethodId => MethodId.ChannelFlowOk;

    public bool Active { get; }
}