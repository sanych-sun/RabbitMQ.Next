using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Transport.Messaging;

internal static class MessageHeader
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteMessageProperties(this IBinaryWriter builder, IMessageProperties properties)
    {
        builder.Allocate(sizeof(ushort), out var flagsBuffer);
        var flags = MessageFlags.None;

        if (!string.IsNullOrEmpty(properties.ContentType))
        {
            flags |= MessageFlags.ContentType;
            builder.Write(properties.ContentType);
        }

        if (!string.IsNullOrEmpty(properties.ContentEncoding))
        {
            flags |= MessageFlags.ContentEncoding;
            builder.Write(properties.ContentEncoding);
        }

        if (properties.Headers != null && properties.Headers.Count > 0)
        {
            flags |= MessageFlags.Headers;
            builder.Write(properties.Headers);
        }

        if (properties.DeliveryMode != DeliveryMode.Unset)
        {
            flags |= MessageFlags.DeliveryMode;
            builder.Write((byte)properties.DeliveryMode);
        }

        if (properties.Priority > 0)
        {
            flags |= MessageFlags.Priority;
            builder.Write(properties.Priority);
        }

        if (!string.IsNullOrEmpty(properties.CorrelationId))
        {
            flags |= MessageFlags.CorrelationId;
            builder.Write(properties.CorrelationId);
        }

        if (!string.IsNullOrEmpty(properties.ReplyTo))
        {
            flags |= MessageFlags.ReplyTo;
            builder.Write(properties.ReplyTo);
        }

        if (!string.IsNullOrEmpty(properties.Expiration))
        {
            flags |= MessageFlags.Expiration;
            builder.Write(properties.Expiration);
        }

        if (!string.IsNullOrEmpty(properties.MessageId))
        {
            flags |= MessageFlags.MessageId;
            builder.Write(properties.MessageId);
        }

        if (properties.Timestamp != default)
        {
            flags |= MessageFlags.Timestamp;
            builder.Write(properties.Timestamp);
        }

        if (!string.IsNullOrEmpty(properties.Type))
        {
            flags |= MessageFlags.Type;
            builder.Write(properties.Type);
        }

        if (!string.IsNullOrEmpty(properties.UserId))
        {
            flags |= MessageFlags.UserId;
            builder.Write(properties.UserId);
        }

        if (!string.IsNullOrEmpty(properties.ApplicationId))
        {
            flags |= MessageFlags.ApplicationId;
            builder.Write(properties.ApplicationId);
        }
        
        BinaryPrimitives.WriteUInt16BigEndian(flagsBuffer.Span, (ushort)flags);
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
        if (source.IsEmpty)
        {
            value = ReadOnlyMemory<byte>.Empty;
            return source;
        }

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
        if (source.IsEmpty)
        {
            value = ReadOnlyMemory<byte>.Empty;
            return source;
        }

        if ((flags & flag) == MessageFlags.None)
        {
            value = ReadOnlyMemory<byte>.Empty;
            return source;
        }

        value = source[..size];
        return source[size..];
    }
}