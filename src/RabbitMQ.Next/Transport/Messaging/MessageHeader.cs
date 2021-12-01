using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal static class MessageHeader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteMessageProperties(this Span<byte> target, IMessageProperties properties)
        {
            var flags = properties.Flags;
            var buffer = target.Write((ushort)flags);

            if (HasFlag(flags, MessageFlags.ContentType))
            {
                buffer = buffer.Write(properties.ContentType);
            }

            if (HasFlag(flags, MessageFlags.ContentEncoding))
            {
                buffer = buffer.Write(properties.ContentEncoding);
            }

            if (HasFlag(flags, MessageFlags.Headers))
            {
                buffer = buffer.Write(properties.Headers);
            }

            if (HasFlag(flags, MessageFlags.DeliveryMode))
            {
                buffer = buffer.Write((byte)properties.DeliveryMode);
            }

            if (HasFlag(flags, MessageFlags.Priority))
            {
                buffer = buffer.Write(properties.Priority);
            }

            if (HasFlag(flags, MessageFlags.CorrelationId))
            {
                buffer = buffer.Write(properties.CorrelationId);
            }

            if (HasFlag(flags, MessageFlags.ReplyTo))
            {
                buffer = buffer.Write(properties.ReplyTo);
            }

            if (HasFlag(flags, MessageFlags.Expiration))
            {
                buffer = buffer.Write(properties.Expiration);
            }

            if (HasFlag(flags, MessageFlags.MessageId))
            {
                buffer = buffer.Write(properties.MessageId);
            }

            if (HasFlag(flags, MessageFlags.Timestamp))
            {
                buffer = buffer.Write(properties.Timestamp);
            }

            if (HasFlag(flags, MessageFlags.Type))
            {
                buffer = buffer.Write(properties.Type);
            }

            if (HasFlag(flags, MessageFlags.UserId))
            {
                buffer = buffer.Write(properties.UserId);
            }

            if (HasFlag(flags, MessageFlags.ApplicationId))
            {
                buffer = buffer.Write(properties.ApplicationId);
            }

            return target.Length - buffer.Length;
        }

        public static ReadOnlyMemory<byte> SplitStringProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, ushort flags, byte bitNumber)
        {
            if (!BitConverter.IsFlagSet(flags, bitNumber))
            {
                value = ReadOnlyMemory<byte>.Empty;
                return source;
            }

            source.Span.Read(out byte size);

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

            source.Span.Read(out uint size);

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
        private static bool HasFlag(MessageFlags flags, MessageFlags flag) => (flags & flag) == flag;
    }
}