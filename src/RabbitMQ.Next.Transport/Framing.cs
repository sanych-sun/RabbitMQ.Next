using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<byte> WriteMessageProperties(this Span<byte> target, MessageProperties properties)
        {
            var flagsSpan = target;
            target = target.Slice(sizeof(ushort));

            var flags = (ushort)0;

            target = target
                .WriteProperty(properties.ContentType, ref flags, 14)
                .WriteProperty(properties.ContentEncoding, ref flags, 13)
                .WriteProperty(properties.Headers, ref flags, 12)
                .WriteProperty((byte) properties.DeliveryMode, ref flags, 11)
                .WriteProperty(properties.Priority, ref flags, 10)
                .WriteProperty(properties.CorrelationId, ref flags, 9)
                .WriteProperty(properties.ReplyTo, ref flags, 8)
                .WriteProperty(properties.Expiration, ref flags, 7)
                .WriteProperty(properties.MessageId, ref flags, 6)
                .WriteProperty(properties.Timestamp, ref flags, 5)
                .WriteProperty(properties.Type, ref flags, 4)
                .WriteProperty(properties.UserId, ref flags, 3)
                .WriteProperty(properties.ApplicationId, ref flags, 2);

            flagsSpan.Write(flags);

            return target;
        }

        private static Span<byte> WriteProperty(this Span<byte> target, string value, ref ushort flags, byte bitNumber)
        {
            if (string.IsNullOrEmpty(value))
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        private static Span<byte> WriteProperty(this Span<byte> target, IReadOnlyDictionary<string, object> value, ref ushort flags, byte bitNumber)
        {
            if (value == null)
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        private static Span<byte> WriteProperty(this Span<byte> target, byte value, ref ushort flags, byte bitNumber)
        {
            if (value == 0)
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        private static Span<byte> WriteProperty(this Span<byte> target, DateTimeOffset value, ref ushort flags, byte bitNumber)
        {
            if (value == default)
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }
    }
}