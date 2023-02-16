using System;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Transport;

internal static partial class Framing
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
            type = FrameType.None;
            channel = 0;
            payloadSize = 0;

            return;
        }

        type = (FrameType)typeRaw;

        data.Read(out channel)
            .Read(out payloadSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> WriteFrameHeader(this Span<byte> buffer, FrameType type, ushort channel, uint payloadSize)
        => buffer.Write((byte)type)
            .Write(channel)
            .Write(payloadSize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> WriteFrameEnd(this Span<byte> target)
        => target.Write(ProtocolConstants.FrameEndByte);
}