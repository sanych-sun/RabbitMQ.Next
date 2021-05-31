using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct TuneOkMethod : IOutgoingMethod
    {
        public TuneOkMethod(ushort channelMax, uint maxFrameSize, ushort heartbeatInterval)
        {
            this.ChannelMax = channelMax;
            this.MaxFrameSize = maxFrameSize;
            this.HeartbeatInterval = heartbeatInterval;
        }

        public MethodId MethodId => MethodId.ConnectionTuneOk;

        public ushort ChannelMax { get; }

        public uint MaxFrameSize { get; }

        public ushort HeartbeatInterval { get; }
    }
}