using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Transport.Messaging;

internal sealed class LazyMessageProperties : IMessageProperties
{
    private ReadOnlyMemory<byte> contentType;
    private ReadOnlyMemory<byte> contentEncoding;
    private ReadOnlyMemory<byte> headers;
    private ReadOnlyMemory<byte> deliveryMode;
    private ReadOnlyMemory<byte> priority;
    private ReadOnlyMemory<byte> correlationId;
    private ReadOnlyMemory<byte> replyTo;
    private ReadOnlyMemory<byte> expiration;
    private ReadOnlyMemory<byte> messageId;
    private ReadOnlyMemory<byte> timestamp;
    private ReadOnlyMemory<byte> type;
    private ReadOnlyMemory<byte> userId;
    private ReadOnlyMemory<byte> applicationId;

    public void Set(ReadOnlyMemory<byte> buffer)
    {
        buffer.Span.Read(out ushort fl);
        this.Flags = (MessageFlags)fl;

        buffer[sizeof(ushort)..]
            .SplitStringProperty(out this.contentType, this.Flags, MessageFlags.ContentType)
            .SplitStringProperty(out this.contentEncoding, this.Flags, MessageFlags.ContentEncoding)
            .SplitDynamicProperty(out this.headers, this.Flags, MessageFlags.Headers)
            .SplitFixedProperty(out this.deliveryMode, this.Flags, MessageFlags.DeliveryMode, sizeof(byte))
            .SplitFixedProperty(out this.priority, this.Flags, MessageFlags.Priority, sizeof(byte))
            .SplitStringProperty(out this.correlationId, this.Flags, MessageFlags.CorrelationId)
            .SplitStringProperty(out this.replyTo, this.Flags, MessageFlags.ReplyTo)
            .SplitStringProperty(out this.expiration, this.Flags, MessageFlags.Expiration)
            .SplitStringProperty(out this.messageId, this.Flags, MessageFlags.MessageId)
            .SplitFixedProperty(out this.timestamp, this.Flags, MessageFlags.Timestamp, sizeof(ulong))
            .SplitStringProperty(out this.type, this.Flags, MessageFlags.Type)
            .SplitStringProperty(out this.userId, this.Flags, MessageFlags.UserId)
            .SplitStringProperty(out this.applicationId, this.Flags, MessageFlags.ApplicationId);
    }

    public void Reset()
    {
        this.Flags = MessageFlags.None;
        this.contentType = ReadOnlyMemory<byte>.Empty;
        this.contentEncoding = ReadOnlyMemory<byte>.Empty;
        this.headers = ReadOnlyMemory<byte>.Empty;
        this.deliveryMode = ReadOnlyMemory<byte>.Empty;
        this.priority = ReadOnlyMemory<byte>.Empty;
        this.correlationId = ReadOnlyMemory<byte>.Empty;
        this.replyTo = ReadOnlyMemory<byte>.Empty;
        this.expiration = ReadOnlyMemory<byte>.Empty;
        this.messageId = ReadOnlyMemory<byte>.Empty;
        this.timestamp = ReadOnlyMemory<byte>.Empty;
        this.type = ReadOnlyMemory<byte>.Empty;
        this.userId = ReadOnlyMemory<byte>.Empty;
        this.applicationId = ReadOnlyMemory<byte>.Empty;
    }

    public MessageFlags Flags { get; private set; }

    public string ContentType => DecodeString(this.contentType);

    public string ContentEncoding => DecodeString(this.contentEncoding);

    public IReadOnlyDictionary<string, object> Headers => DecodeTable(this.headers);

    public DeliveryMode DeliveryMode => (DeliveryMode)DecodeByte(this.deliveryMode);

    public byte Priority => DecodeByte(this.priority);

    public string CorrelationId => DecodeString(this.correlationId);

    public string ReplyTo => DecodeString(this.replyTo);

    public string Expiration => DecodeString(this.expiration);

    public string MessageId => DecodeString(this.messageId);

    public DateTimeOffset Timestamp => DecodeTimestamp(this.timestamp);

    public string Type => DecodeString(this.type);

    public string UserId => DecodeString(this.userId);

    public string ApplicationId => DecodeString(this.applicationId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string DecodeString(ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty)
        {
            return null;
        }

        return TextEncoding.GetString(data.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTimeOffset DecodeTimestamp(ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty)
        {
            return default;
        }

        data.Span.Read(out DateTimeOffset val);
        return val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IReadOnlyDictionary<string, object> DecodeTable(ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty)
        {
            return null;
        }

        data.Span.Read(out Dictionary<string, object> val);
        return val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte DecodeByte(ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty)
        {
            return default;
        }

        data.Span.Read(out byte val);
        return val;
    }
}