using System;

namespace RabbitMQ.Next.Transport
{
    internal static class ProtocolConstants
    {
        public const int FrameMinSize = 4096;

        public const int FrameHeaderSize = 7;
        public const byte FrameEndByte = 0xCE;
        public const byte ObsoleteField = 0x00;
        public const string DefaultVHost = "/";

        public static readonly ReadOnlyMemory<byte> AmqpHeader = new byte[] { 0x41, 0x4D, 0x51, 0x50, 0x00, 0x00, 0x09, 0x01 };
    }
}