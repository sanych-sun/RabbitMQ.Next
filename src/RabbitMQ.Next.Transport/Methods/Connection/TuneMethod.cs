using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct TuneMethod : IIncomingMethod
    {
        public TuneMethod(ushort channelMax, uint maxFrameSize, ushort heartbeatInterval)
        {
            this.ChannelMax = channelMax;
            this.MaxFrameSize = maxFrameSize;
            this.HeartbeatInterval = heartbeatInterval;
        }

        public uint Method => (uint) MethodId.ConnectionTune;

        public ushort ChannelMax { get; }

        public uint MaxFrameSize { get; }

        public ushort HeartbeatInterval { get; }
    }
}