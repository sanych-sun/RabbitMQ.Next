using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Transport;

internal static partial class Framing
{
    private const uint ContentHeaderPrefix = (ushort)ClassId.Basic << 16;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> WriteContentHeader(this Span<byte> buffer, IMessageProperties properties, out Span<byte> contentSizeBuffer)
    {
        buffer = buffer
            .Write(ContentHeaderPrefix)
            .Slice(sizeof(ulong), out contentSizeBuffer)
            .Slice(sizeof(ushort), out var flagsBuffer);
        
        var flags = MessageFlags.None;

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

        flagsBuffer.Write((ushort)flags);
        return buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SplitContentHeaderProperties(
        this ReadOnlyMemory<byte> data,
        out ReadOnlyMemory<byte> contentType,
        out ReadOnlyMemory<byte> contentEncoding,
        out ReadOnlyMemory<byte> headers,
        out ReadOnlyMemory<byte> deliveryMode,
        out ReadOnlyMemory<byte> priority,
        out ReadOnlyMemory<byte> correlationId,
        out ReadOnlyMemory<byte> replyTo,
        out ReadOnlyMemory<byte> expiration,
        out ReadOnlyMemory<byte> messageId,
        out ReadOnlyMemory<byte> timestamp,
        out ReadOnlyMemory<byte> type,
        out ReadOnlyMemory<byte> userId,
        out ReadOnlyMemory<byte> applicationId) 
    {
        data.Span.Read(out ushort fl);
        var flags = (MessageFlags)fl;

        data[sizeof(ushort)..]
            .SplitStringProperty(out contentType, flags, MessageFlags.ContentType)
            .SplitStringProperty(out contentEncoding, flags, MessageFlags.ContentEncoding)
            .SplitDynamicProperty(out headers, flags, MessageFlags.Headers)
            .SplitFixedProperty(out deliveryMode, flags, MessageFlags.DeliveryMode, sizeof(byte))
            .SplitFixedProperty(out priority, flags, MessageFlags.Priority, sizeof(byte))
            .SplitStringProperty(out correlationId, flags, MessageFlags.CorrelationId)
            .SplitStringProperty(out replyTo, flags, MessageFlags.ReplyTo)
            .SplitStringProperty(out expiration, flags, MessageFlags.Expiration)
            .SplitStringProperty(out messageId, flags, MessageFlags.MessageId)
            .SplitFixedProperty(out timestamp, flags, MessageFlags.Timestamp, sizeof(ulong))
            .SplitStringProperty(out type, flags, MessageFlags.Type)
            .SplitStringProperty(out userId, flags, MessageFlags.UserId)
            .SplitStringProperty(out applicationId, flags, MessageFlags.ApplicationId);
        
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlyMemory<byte> SplitStringProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, MessageFlags flags, MessageFlags flag)
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
    private static ReadOnlyMemory<byte> SplitDynamicProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, MessageFlags flags, MessageFlags flag)
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
    private static ReadOnlyMemory<byte> SplitFixedProperty(this ReadOnlyMemory<byte> source, out ReadOnlyMemory<byte> value, MessageFlags flags, MessageFlags flag, int size)
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