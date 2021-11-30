using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Transport.Messaging
{
    internal static class MessageHeader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> WriteMessageProperties(this Memory<byte> target, IMessageProperties properties)
        {
            var flags = properties.Flags;
            target = target.Write((ushort)flags);

            if (HasFlag(flags, MessageFlags.ContentType))
            {
                target = target.Write(properties.ContentType);
            }

            if (HasFlag(flags, MessageFlags.ContentEncoding))
            {
                target = target.Write(properties.ContentEncoding);
            }

            if (HasFlag(flags, MessageFlags.Headers))
            {
                target = target.Write(properties.Headers);
            }

            if (HasFlag(flags, MessageFlags.DeliveryMode))
            {
                target = target.Write((byte)properties.DeliveryMode);
            }

            if (HasFlag(flags, MessageFlags.Priority))
            {
                target = target.Write(properties.Priority);
            }

            if (HasFlag(flags, MessageFlags.CorrelationId))
            {
                target = target.Write(properties.CorrelationId);
            }

            if (HasFlag(flags, MessageFlags.ReplyTo))
            {
                target = target.Write(properties.ReplyTo);
            }

            if (HasFlag(flags, MessageFlags.Expiration))
            {
                target = target.Write(properties.Expiration);
            }

            if (HasFlag(flags, MessageFlags.MessageId))
            {
                target = target.Write(properties.MessageId);
            }

            if (HasFlag(flags, MessageFlags.Timestamp))
            {
                target = target.Write(properties.Timestamp);
            }

            if (HasFlag(flags, MessageFlags.Type))
            {
                target = target.Write(properties.Type);
            }

            if (HasFlag(flags, MessageFlags.UserId))
            {
                target = target.Write(properties.UserId);
            }

            if (HasFlag(flags, MessageFlags.ApplicationId))
            {
                target = target.Write(properties.ApplicationId);
            }

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
        private static bool HasFlag(MessageFlags flags, MessageFlags flag) => (flags & flag) == flag;
    }
}