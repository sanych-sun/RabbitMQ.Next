namespace RabbitMQ.Next.Transport
{
    internal readonly struct ConnectionNegotiationResults
    {
        public ConnectionNegotiationResults(ushort channelMax, uint maxFrameSize, ushort heartbeatInterval)
        {
            this.ChannelMax = channelMax;
            this.MaxFrameSize = maxFrameSize;
            this.HeartbeatInterval = heartbeatInterval;
        }

        public ushort ChannelMax { get; }

        public uint MaxFrameSize { get; }

        public ushort HeartbeatInterval { get; }
    }
}