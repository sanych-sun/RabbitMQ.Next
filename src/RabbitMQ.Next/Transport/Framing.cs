using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Transport
{
    internal static class Framing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadFrameHeader(this ReadOnlyMemory<byte> data, out FrameType type, out ushort channel, out uint payloadSize)
        {
            data = data.Read(out byte typeRaw);

            if (typeRaw != (byte)FrameType.Method
                && typeRaw != (byte)FrameType.ContentHeader
                && typeRaw != (byte)FrameType.ContentBody
                && typeRaw != (byte)FrameType.Heartbeat)
            {
                type = FrameType.Malformed;
                channel = 0;
                payloadSize = 0;

                return;
            }

            type = (FrameType)typeRaw;

            data.Read(out channel)
                .Read(out payloadSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFrameHeader(this Memory<byte> target, FrameType type, ushort channel, uint payloadSize)
            => target.Write((byte)type)
                .Write(channel)
                .Write(payloadSize);
    }
}