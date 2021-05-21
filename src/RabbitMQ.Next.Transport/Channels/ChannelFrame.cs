using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Transport.Channels
{
    internal static class ChannelFrame
    {
        public const int FrameHeaderSize = 5; // type (byte) + size (int)

        public static void WriteChannelHeader(this IBufferWriter<byte> buffer, ChannelFrameType type, int size)
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

        public static (FrameType, int) ReadChannelHeader(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsSingleSegment)
            {
                return sequence.FirstSpan.ReadChannelHeader();
            }

            Span<byte> headerBuffer = stackalloc byte[ProtocolConstants.FrameHeaderSize];
            sequence.Slice(0, ProtocolConstants.FrameHeaderSize).CopyTo(headerBuffer);
            return ((ReadOnlySpan<byte>) headerBuffer).ReadChannelHeader();
        }
    }
}