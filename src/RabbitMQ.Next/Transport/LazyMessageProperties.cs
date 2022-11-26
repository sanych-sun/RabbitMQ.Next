using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Transport;

internal sealed class LazyMessageProperties : IMessageProperties
{
    private ReadOnlyMemory<byte> headerBytes;
    private bool isSplit;

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

    public void Set(ReadOnlyMemory<byte> data)
    {
        this.headerBytes = data;
        this.isSplit = false;
    }

    public void Reset()
    {
        this.headerBytes = ReadOnlyMemory<byte>.Empty;
        this.isSplit = false;
        
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
    
    public string ContentType
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.contentType);
        }
    }

    public string ContentEncoding
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.contentEncoding);
        }
    }

    public IReadOnlyDictionary<string, object> Headers
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeTable(this.headers);
        }
    }

    public DeliveryMode DeliveryMode
    {
        get
        {
            this.EnsureMemorySplit();
            return (DeliveryMode)DecodeByte(this.deliveryMode);
        }
    }

    public byte Priority
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeByte(this.priority);
        }
    }

    public string CorrelationId
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.correlationId);
        }
    }

    public string ReplyTo
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.replyTo);
        }
    }

    public string Expiration
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.expiration);
        }
    }

    public string MessageId
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.messageId);
        }
    }

    public DateTimeOffset Timestamp
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeTimestamp(this.timestamp);
        }
    }

    public string Type
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.type);
        }
    }

    public string UserId
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.userId);
        }
    }

    public string ApplicationId
    {
        get
        {
            this.EnsureMemorySplit();
            return DecodeString(this.applicationId);
        }
    }

    private void EnsureMemorySplit()
    {
        if (this.isSplit)
        {
            return;
        }

        if (this.headerBytes.IsEmpty)
        {
            throw new InvalidOperationException();
        }

        this.headerBytes.SplitContentHeaderProperties(
            out this.contentType,
            out this.contentEncoding,
            out this.headers,
            out this.deliveryMode,
            out this.priority,
            out this.correlationId,
            out this.replyTo,
            out this.expiration,
            out this.messageId,
            out this.timestamp,
            out this.type,
            out this.userId,
            out this.applicationId);
        this.isSplit = true;
    }
    
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