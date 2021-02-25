using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public static class ContentHeader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteContentHeader(this Span<byte> target, IMessageProperties properties, ulong contentSize)
        {
            var result = target
                .Write((ushort) ClassId.Basic)
                .Write((ushort) ProtocolConstants.ObsoleteField)
                .Write(contentSize)
                .WriteMessageProperties(properties);

            return target.Length - result.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteMessageProperties(this Span<byte> target, IMessageProperties properties)
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
        public static ReadOnlySpan<byte> ReadMessageProperties(this ReadOnlySpan<byte> source, out IMessageProperties properties)
        {
            // TODO: might want to read from ReadOnlySequence
            source = source
                .Read(out ushort flags)
                .ReadProperty(out string contentType, flags, 15)
                .ReadProperty(out string contentEncoding, flags, 14)
                .ReadProperty(out Dictionary<string, object> headers, flags, 13)
                .ReadProperty(out byte deliveryMode, flags, 12)
                .ReadProperty(out byte? priority, flags, 11)
                .ReadProperty(out string correlationId, flags, 10)
                .ReadProperty(out string replyTo, flags, 9)
                .ReadProperty(out string expiration, flags, 8)
                .ReadProperty(out string messageId, flags, 7)
                .ReadProperty(out DateTimeOffset? timestamp, flags, 6)
                .ReadProperty(out string type, flags, 5)
                .ReadProperty(out string userId, flags, 4)
                .ReadProperty(out string applicationId, flags, 3);

            properties = new MessageProperties
            {
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                Headers = headers,
                DeliveryMode = (DeliveryMode)deliveryMode,
                Priority = priority,
                CorrelationId = correlationId,
                ReplyTo = replyTo,
                Expiration = expiration,
                MessageId = messageId,
                Timestamp = timestamp,
                Type = type,
                UserId = userId,
                ApplicationId = applicationId
            };
            return source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<byte> ReadProperty(this ReadOnlySpan<byte> source, out string value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = default;
                return source;
            }

            return source.Read(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<byte> ReadProperty(this ReadOnlySpan<byte> source, out Dictionary<string, object> value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = default;
                return source;
            }

            return source.Read(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<byte> ReadProperty(this ReadOnlySpan<byte> source, out byte value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = default;
                return source;
            }

            return source.Read(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<byte> ReadProperty(this ReadOnlySpan<byte> source, out byte? value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = default;
                return source;
            }

            var result = source.Read(out byte data);
            value = data;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<byte> ReadProperty(this ReadOnlySpan<byte> source, out DateTimeOffset? value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = null;
                return source;
            }

            var result = source.Read(out DateTimeOffset val);
            value = val;
            return result;
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