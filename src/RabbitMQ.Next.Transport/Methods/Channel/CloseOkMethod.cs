using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    public readonly struct CloseOkMethod : IIncomingMethod, IOutgoingMethod
    {
        public uint Method => (uint) MethodId.ChannelCloseOk;
    }
}