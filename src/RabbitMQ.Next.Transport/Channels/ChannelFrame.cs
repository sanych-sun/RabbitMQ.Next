using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport.Channels
{
    internal static class ChannelFrame
    {
        public const int FrameHeaderSize = 5; // type (byte) + size (int)

        public static void WriteChannelHeader(this IBufferWriter<byte> buffer, FrameType type, int size)
        {
            buffer.GetSpan(FrameHeaderSize)
                .Write((byte) type)
                .Write(size);
            buffer.Advance(FrameHeaderSize);
        }

        public static (FrameType FrameType, int Size) ReadChannelHeader(this ReadOnlySpan<byte> payload)
        {
            byte type;
            int size;

            payload
                .Read(out type)
                .Read(out size);

            return ((FrameType)type, size);
        }
    }
}