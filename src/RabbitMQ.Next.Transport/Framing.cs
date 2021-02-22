using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport
{
    internal static class Framing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadFrameHeader(this ReadOnlySpan<byte> data, out FrameType type, out ushort channel, out uint payloadSize)
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
        public static int WriteFrameHeader(this Span<byte> target, FrameType type, ushort channel, uint payloadSize)
        {
            var result = target.Write((byte)type)
                .Write(channel)
                .Write(payloadSize);

            return target.Length - result.Length;
        }
    }
}