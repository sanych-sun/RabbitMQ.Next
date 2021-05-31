using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport.Channels
{
    internal static class ChannelFrame
    {
        public const int FrameHeaderSize = 5; // type (byte) + size (uint)

        public static void WriteHeader(IBufferWriter<byte> buffer, ChannelFrameType type, uint size)
        {
            buffer.GetSpan(FrameHeaderSize)
                .Write((byte) type)
                .Write(size);
            buffer.Advance(FrameHeaderSize);
        }

        public static (ChannelFrameType Type, uint Size) ReadHeader(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsSingleSegment)
            {
                return ReadHeader(sequence.FirstSpan);
            }

            Span<byte> headerBuffer = stackalloc byte[FrameHeaderSize];
            sequence.Slice(0, FrameHeaderSize).CopyTo(headerBuffer);
            return ReadHeader(headerBuffer);
        }

        private static (ChannelFrameType FrameType, uint Size) ReadHeader(ReadOnlySpan<byte> payload)
        {
            payload
                .Read(out byte type)
                .Read(out uint size);

            return ((ChannelFrameType)type, size);
        }
    }
}