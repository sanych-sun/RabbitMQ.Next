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

        public uint Method => (uint) MethodId.ConnectionTuneOk;

        public ushort ChannelMax { get; }

        public uint MaxFrameSize { get; }

        public ushort HeartbeatInterval { get; }
    }
}