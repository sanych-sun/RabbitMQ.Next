using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal static class ContentHeader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteContentHeader(this Memory<byte> target, IMessageProperties properties, ulong contentSize)
        {
            var result = target
                .Write((ushort) ClassId.Basic)
                .Write((ushort) ProtocolConstants.ObsoleteField)
                .Write(contentSize)
                .WriteMessageProperties(properties);

            return target.Length - result.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> WriteMessageProperties(this Memory<byte> target, IMessageProperties properties)
        {
            var flagsSpan = target;
            target = target.Slice(sizeof(ushort));

            var flags = (ushort)0;

            if (properties != null)
            {
                target = target
                    .WriteProperty(properties.ContentType, ref flags, 15)
                    .WriteProperty(properties.ContentEncoding, ref flags, 14)
                    .WriteProperty(properties.Headers, ref flags, 13)
                    .WriteProperty((byte) properties.DeliveryMode, ref flags, 12)
                    .WriteProperty(properties.Priority, ref flags, 11)
                    .WriteProperty(properties.CorrelationId, ref flags, 10)
                    .WriteProperty(properties.ReplyTo, ref flags, 9)
                    .WriteProperty(properties.Expiration, ref flags, 8)
                    .WriteProperty(properties.MessageId, ref flags, 7)
                    .WriteProperty(properties.Timestamp, ref flags, 6)
                    .WriteProperty(properties.Type, ref flags, 5)
                    .WriteProperty(properties.UserId, ref flags, 4)
                    .WriteProperty(properties.ApplicationId, ref flags, 3);
            }

            flagsSpan.Write(flags);

            return target;
        }

        public static ReadOnlyMemory<byte> SplitStringProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlyMemory<byte>.Empty;
                return source;
            }

            source.Read(out byte size);

            value = source.Slice(sizeof(byte), size);
            return source.Slice(sizeof(byte) + size);
        }

        public static ReadOnlyMemory<byte> SplitTableProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlyMemory<byte>.Empty;
                return source;
            }

            source.Read(out uint size);

            value = source.Slice(0, sizeof(uint) + (int)size);
            return source.Slice(sizeof(uint) + (int)size);
        }

        public static ReadOnlyMemory<byte> SplitFixedSizeProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, ushort flags, byte bitNumber, int size)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlyMemory<byte>.Empty;
                return source;
            }

            value = source.Slice(0, size);
            return source.Slice(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Memory<byte> WriteProperty(this Memory<byte> target, string value, ref ushort flags, byte bitNumber)
        {
            if (string.IsNullOrEmpty(value))
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Memory<byte> WriteProperty(this Memory<byte> target, IReadOnlyDictionary<string, object> value, ref ushort flags, byte bitNumber)
        {
            if (value == null || value.Count == 0)
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Memory<byte> WriteProperty(this Memory<byte> target, byte? value, ref ushort flags, byte bitNumber)
        {
            if (!value.HasValue)
            {
                return target;
            }

            return target.WriteProperty(value.Value, ref flags, bitNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Memory<byte> WriteProperty(this Memory<byte> target, byte value, ref ushort flags, byte bitNumber)
        {
            if (value == default)
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Memory<byte> WriteProperty(this Memory<byte> target, DateTimeOffset? value, ref ushort flags, byte bitNumber)
        {
            if (!value.HasValue)
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value.Value);
        }
    }
}