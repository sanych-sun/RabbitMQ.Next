using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport
{
    internal static class Framing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FrameHeader ReadFrameHeader(this ReadOnlySpan<byte> data)
        {
            data = data.Read(out byte typeRaw);

            if (typeRaw != (byte)FrameType.Method
                && typeRaw != (byte)FrameType.ContentHeader
                && typeRaw != (byte)FrameType.ContentBody
                && typeRaw != (byte)FrameType.Heartbeat)
            {
                return new FrameHeader(FrameType.Malformed, 0, 0);
            }

            var type = (FrameType) typeRaw;

            data.Read(out ushort channel)
                .Read(out uint size);

            return new FrameHeader(type, channel, (int) size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteFrameHeader(this Span<byte> target, FrameHeader header)
        {
            var result = target.Write((byte)header.Type)
                .Write(header.Channel)
                .Write((uint)header.PayloadSize);

            return target.Length - result.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteContentHeader(this Span<byte> target, MessageProperties properties, ulong contentSize)
        {
            var result = target
                .Write((ushort) ClassId.Basic)
                .Write((ushort) ProtocolConstants.ObsoleteField)
                .Write(contentSize)
                .WriteMessageProperties(properties);

            return target.Length - result.Length;
        }
    }
}