using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal static class MessageHeader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> WriteMessageProperties(this Memory<byte> target, IMessageProperties properties)
        {
            var flagsSpan = target;
            target = target[sizeof(ushort)..];

            var flags = (ushort)0;

            if (properties != null)
            {
                target = target
                    .WriteProperty(properties.ContentType, ref flags, MessagePropertiesBits.ContentType)
                    .WriteProperty(properties.ContentEncoding, ref flags, MessagePropertiesBits.ContentEncoding)
                    .WriteProperty(properties.Headers, ref flags, MessagePropertiesBits.Headers)
                    .WriteProperty((byte) properties.DeliveryMode, ref flags, MessagePropertiesBits.DeliveryMode)
                    .WriteProperty(properties.Priority, ref flags, MessagePropertiesBits.Priority)
                    .WriteProperty(properties.CorrelationId, ref flags, MessagePropertiesBits.CorrelationId)
                    .WriteProperty(properties.ReplyTo, ref flags, MessagePropertiesBits.ReplyTo)
                    .WriteProperty(properties.Expiration, ref flags, MessagePropertiesBits.Expiration)
                    .WriteProperty(properties.MessageId, ref flags, MessagePropertiesBits.MessageId)
                    .WriteProperty(properties.Timestamp, ref flags, MessagePropertiesBits.Timestamp)
                    .WriteProperty(properties.Type, ref flags, MessagePropertiesBits.Type)
                    .WriteProperty(properties.UserId, ref flags, MessagePropertiesBits.UserId)
                    .WriteProperty(properties.ApplicationId, ref flags, MessagePropertiesBits.ApplicationId);
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
            return source[(sizeof(byte) + size)..];
        }

        public static ReadOnlyMemory<byte> SplitDynamicProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlyMemory<byte>.Empty;
                return source;
            }

            source.Read(out uint size);

            value = source[..(sizeof(uint) + (int)size)];
            return source[(sizeof(uint) + (int)size)..];
        }

        public static ReadOnlyMemory<byte> SplitFixedProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, ushort flags, byte bitNumber, int size)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlyMemory<byte>.Empty;
                return source;
            }

            value = source[..size];
            return source[size..];
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