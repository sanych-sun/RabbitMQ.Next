namespace RabbitMQ.Next.Transport
{
    internal static class ProtocolConstants
    {
        public const int ConnectionChannel = 0;
        public const int FrameMinSize = 4096;

        public const int FrameHeaderSize = 7;
        public const byte FrameEndByte = 0xCE;
        public const byte ObsoleteField = 0x00;
        public const string DefaultVHost = "/";

        public static readonly byte[] HeartbeatFrame = { (byte)FrameType.Heartbeat, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, FrameEndByte };
        public static readonly byte[] AmqpHeader = { 0x41, 0x4D, 0x51, 0x50, 0x00, 0x00, 0x09, 0x01 };
    }
}