using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel;

public readonly struct CloseOkMethod : IIncomingMethod, IOutgoingMethod
{
    public MethodId MethodId => MethodId.ChannelCloseOk;
}