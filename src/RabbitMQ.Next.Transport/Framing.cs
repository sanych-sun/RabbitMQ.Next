using System;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Transport
{
    internal static class Framing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static FrameHeader ReadFrameHeader(this ReadOnlySpan<byte> data)
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
        internal static Span<byte> WriteFrameHeader(this Span<byte> target, FrameHeader header)
        {
            return target.Write((byte)header.Type)
                .Write(header.Channel)
                .Write((uint)header.PayloadSize);
        }
    }
}