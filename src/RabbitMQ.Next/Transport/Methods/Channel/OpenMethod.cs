using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    public readonly struct OpenMethod : IOutgoingMethod
    {
        public MethodId MethodId => MethodId.ChannelOpen;
    }
}