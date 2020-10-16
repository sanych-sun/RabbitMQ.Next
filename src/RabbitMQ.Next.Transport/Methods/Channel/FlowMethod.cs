using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel
{
    public readonly struct FlowMethod : IIncomingMethod, IOutgoingMethod
    {
        public FlowMethod(bool active)
        {
            this.Active = active;
        }

        public uint Method => (uint) MethodId.ChannelFlow;

        public bool Active { get; }
    }
}