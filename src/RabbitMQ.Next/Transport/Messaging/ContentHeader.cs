using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal static class ContentHeader
    {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteMessageProperties(this Span<byte> target, MessageProperties properties)
        {
            var flagsSpan = target;
            target = target.Slice(sizeof(ushort));

            var flags = (ushort)0;

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

            flagsSpan.Write(flags);

            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlyMemory<byte> ReadProperty(this ReadOnlyMemory<byte> source, out StringProperty value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = new StringProperty(ReadOnlyMemory<byte>.Empty);
                return source;
            }

            source.Span.Read(out byte size);
            value = new StringProperty(source.Slice(0, size + sizeof(byte)));

            return source.Slice(size + sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlyMemory<byte> ReadProperty(this ReadOnlyMemory<byte> source, out TableProperty value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = new TableProperty(ReadOnlyMemory<byte>.Empty);
                return source;
            }

            source.Span.Read(out uint size);
            value = new TableProperty(source.Slice(0, (int)size + sizeof(uint)));
            return source.Slice((int)size + sizeof(uint));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlyMemory<byte> ReadProperty(this ReadOnlyMemory<byte> source, out byte? value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = default;
                return source;
            }

            source.Span.Read(out byte data);
            value = data;
            return source.Slice(sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlyMemory<byte> ReadProperty(this ReadOnlyMemory<byte> source, out DateTimeOffset? value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = null;
                return source;
            }

            source.Span.Read(out DateTimeOffset val);
            value = val;
            return source.Slice(sizeof(long));
        }


        public static ReadOnlySequence<byte> SplitStringProperty(this ReadOnlySequence<byte> source, out ReadOnlySequence<byte> value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlySequence<byte>.Empty;
                return source;
            }

            byte size;
            if (source.FirstSpan.Length >= sizeof(byte))
            {
                source.FirstSpan.Read(out size);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(byte)];
                source.Slice(0, sizeof(byte)).CopyTo(buffer);
                ((ReadOnlySpan<byte>) buffer).Read(out size);
            }

            value = source.Slice(sizeof(byte), size);
            return source.Slice(sizeof(byte) + size);
        }

        public static ReadOnlySequence<byte> SplitTableProperty(this ReadOnlySequence<byte> source, out ReadOnlySequence<byte> value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlySequence<byte>.Empty;
                return source;
            }

            uint size;
            if (source.FirstSpan.Length >= sizeof(uint))
            {
                source.FirstSpan.Read(out size);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                source.Slice(0, sizeof(uint)).CopyTo(buffer);
                ((ReadOnlySpan<byte>) buffer).Read(out size);
            }

            value = source.Slice(0, sizeof(uint) + size);
            return source.Slice(sizeof(uint) + size);
        }

        public static ReadOnlySequence<byte> SplitFixedSizeProperty(this ReadOnlySequence<byte> source, out ReadOnlySequence<byte> value, ushort flags, byte bitNumber, int size)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlySequence<byte>.Empty;
                return source;
            }

            value = source.Slice(0, size);
            return source.Slice(size);
        }

        public static ReadOnlySequence<byte> Read(this ReadOnlySequence<byte> source, out ushort value)
        {
            if (source.FirstSpan.Length > sizeof(ushort))
            {
                source.FirstSpan.Read(out value);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(ushort)];
                source.Slice(0, sizeof(ushort)).CopyTo(buffer);
                ((ReadOnlySpan<byte>) buffer).Read(out value);
            }

            return source.Slice(sizeof(ushort));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Span<byte> WriteProperty(this Span<byte> target, string value, ref ushort flags, byte bitNumber)
        {
            if (string.IsNullOrEmpty(value))
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Span<byte> WriteProperty(this Span<byte> target, IReadOnlyDictionary<string, object> value, ref ushort flags, byte bitNumber)
        {
            if (value == null)
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Span<byte> WriteProperty(this Span<byte> target, byte? value, ref ushort flags, byte bitNumber)
        {
            if (!value.HasValue)
            {
                return target;
            }

            return target.WriteProperty(value.Value, ref flags, bitNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Span<byte> WriteProperty(this Span<byte> target, byte value, ref ushort flags, byte bitNumber)
        {
            if (value == default)
            {
                return target;
            }

            flags = (ushort)(flags | (1 << bitNumber));

            return target.Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Span<byte> WriteProperty(this Span<byte> target, DateTimeOffset? value, ref ushort flags, byte bitNumber)
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