namespace RabbitMQ.Next.Transport
{
    internal readonly struct FrameHeader
    {
        public FrameHeader(FrameType type, ushort channel, int payloadSize)
        {
            this.Type = type;
            this.Channel = channel;
            this.PayloadSize = payloadSize;
        }

        public FrameType Type { get; }

        public ushort Channel { get; }

        public int PayloadSize { get; }

        public bool IsEmpty() => (this.Type == 0);
    }
}