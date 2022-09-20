using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Transport.Messaging;

internal static class MessageHeader
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteMessageProperties(this IBufferBuilder builder, IMessageProperties properties)
    {
        var flags = properties.Flags;
        builder.Write((ushort)flags);

        if ((flags & MessageFlags.ContentType) != MessageFlags.None)
        {
            builder.Write(properties.ContentType);
        }

        if ((flags & MessageFlags.ContentEncoding) != MessageFlags.None)
        {
            builder.Write(properties.ContentEncoding);
        }

        if ((flags & MessageFlags.Headers) != MessageFlags.None)
        {
            builder.Write(properties.Headers);
        }

        if ((flags & MessageFlags.DeliveryMode) != MessageFlags.None)
        {
            builder.Write((byte)properties.DeliveryMode);
        }

        if ((flags & MessageFlags.Priority) != MessageFlags.None)
        {
            builder.Write(properties.Priority);
        }

        if ((flags & MessageFlags.CorrelationId) != MessageFlags.None)
        {
            builder.Write(properties.CorrelationId);
        }

        if ((flags & MessageFlags.ReplyTo) != MessageFlags.None)
        {
            builder.Write(properties.ReplyTo);
        }

        if ((flags & MessageFlags.Expiration) != MessageFlags.None)
        {
            builder.Write(properties.Expiration);
        }

        if ((flags & MessageFlags.MessageId) != MessageFlags.None)
        {
            builder.Write(properties.MessageId);
        }

        if ((flags & MessageFlags.Timestamp) != MessageFlags.None)
        {
            builder.Write(properties.Timestamp);
        }

        if ((flags & MessageFlags.Type) != MessageFlags.None)
        {
            builder.Write(properties.Type);
        }

        if ((flags & MessageFlags.UserId) != MessageFlags.None)
        {
            builder.Write(properties.UserId);
        }

        if ((flags & MessageFlags.ApplicationId) != MessageFlags.None)
        {
            builder.Write(properties.ApplicationId);
        }
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