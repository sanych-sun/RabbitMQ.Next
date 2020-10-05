namespace RabbitMQ.Next.Transport.Methods.Channel
{
    public readonly struct OpenMethod : IOutgoingMethod
    {
        public uint Method => (uint) MethodId.ChannelOpen;
    }
}