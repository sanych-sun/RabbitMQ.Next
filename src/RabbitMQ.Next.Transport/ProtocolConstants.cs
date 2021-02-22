using System;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Transport
{
    internal static class ProtocolConstants
    {
        public const int FrameMinSize = 4096;

        public const int FrameHeaderSize = 7;
        public const byte FrameEndByte = 0xCE;
        public const byte ObsoleteField = 0x00;

        public static readonly ReadOnlyMemory<byte> AmqpHeader = new byte[] { 0x41, 0x4D, 0x51, 0x50, 0x00, 0x00, 0x09, 0x01 };
        public static readonly ReadOnlyMemory<byte> HeartbeatFrame = new byte[] { (byte)FrameType.Heartbeat, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
    }
}