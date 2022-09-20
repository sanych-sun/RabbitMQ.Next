using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Transport;

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
    public static void WriteFrameHeader(Span<byte> buffer, FrameType frameType, ushort channel, uint payloadSize)
    {
        buffer[0] = (byte)frameType;
        BinaryPrimitives.WriteUInt16BigEndian(buffer[1..], channel);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[3..], payloadSize);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteFrameEnd(Span<byte> buffer)
    {
        buffer[0] = ProtocolConstants.FrameEndByte;
    }
}