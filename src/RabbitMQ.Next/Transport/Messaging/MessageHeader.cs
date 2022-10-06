using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Transport.Messaging;

internal static class MessageHeader
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteMessageProperties(this Span<byte> target, IMessageProperties properties)
    {
        var flags = MessageFlags.None;
        
        var buffer = target[sizeof(ushort)..];

        if (!string.IsNullOrEmpty(properties.ContentType))
        {
            flags |= MessageFlags.ContentType;
            buffer = buffer.Write(properties.ContentType);
        }

        if (!string.IsNullOrEmpty(properties.ContentEncoding))
        {
            flags |= MessageFlags.ContentEncoding;
            buffer = buffer.Write(properties.ContentEncoding);
        }

        if (properties.Headers != null && properties.Headers.Count > 0)
        {
            flags |= MessageFlags.Headers;
            buffer = buffer.Write(properties.Headers);
        }

        if (properties.DeliveryMode != DeliveryMode.Unset)
        {
            flags |= MessageFlags.DeliveryMode;
            buffer = buffer.Write((byte)properties.DeliveryMode);
        }

        if (properties.Priority > 0)
        {
            flags |= MessageFlags.Priority;
            buffer = buffer.Write(properties.Priority);
        }

        if (!string.IsNullOrEmpty(properties.CorrelationId))
        {
            flags |= MessageFlags.CorrelationId;
            buffer = buffer.Write(properties.CorrelationId);
        }

        if (!string.IsNullOrEmpty(properties.ReplyTo))
        {
            flags |= MessageFlags.ReplyTo;
            buffer = buffer.Write(properties.ReplyTo);
        }

        if (!string.IsNullOrEmpty(properties.Expiration))
        {
            flags |= MessageFlags.Expiration;
            buffer = buffer.Write(properties.Expiration);
        }

        if (!string.IsNullOrEmpty(properties.MessageId))
        {
            flags |= MessageFlags.MessageId;
            buffer = buffer.Write(properties.MessageId);
        }

        if (properties.Timestamp != default)
        {
            flags |= MessageFlags.Timestamp;
            buffer = buffer.Write(properties.Timestamp);
        }

        if (!string.IsNullOrEmpty(properties.Type))
        {
            flags |= MessageFlags.Type;
            buffer = buffer.Write(properties.Type);
        }

        if (!string.IsNullOrEmpty(properties.UserId))
        {
            flags |= MessageFlags.UserId;
            buffer = buffer.Write(properties.UserId);
        }

        if (!string.IsNullOrEmpty(properties.ApplicationId))
        {
            flags |= MessageFlags.ApplicationId;
            buffer = buffer.Write(properties.ApplicationId);
        }

        target.Write((ushort)flags);
        return target.Length - buffer.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> SplitStringProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, MessageFlags flags, MessageFlags flag)
    {
        if ((flags & flag) == MessageFlags.None)
        {
            value = ReadOnlyMemory<byte>.Empty;
            return source;
        }

        source.Span.Read(out byte size);

        value = source.Slice(sizeof(byte), size);
        return source[(sizeof(byte) + size)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> SplitDynamicProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, MessageFlags flags, MessageFlags flag)
    {
        if ((flags & flag) == MessageFlags.None)
        {
            value = ReadOnlyMemory<byte>.Empty;
            return source;
        }

        source.Span.Read(out uint size);

        value = source[..(sizeof(uint) + (int)size)];
        return source[(sizeof(uint) + (int)size)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> SplitFixedProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, MessageFlags flags, MessageFlags flag, int size)
    {
        if ((flags & flag) == MessageFlags.None)
        {
            value = ReadOnlyMemory<byte>.Empty;
            return source;
        }

        value = source[..size];
        return source[size..];
    }
}