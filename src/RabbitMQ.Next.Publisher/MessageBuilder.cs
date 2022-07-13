using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

public class MessageBuilder : IMessageBuilder, IMessageProperties
{
    private readonly Dictionary<string, object> headers = new();

    public void Reset()
    {
        this.Flags = MessageFlags.DeliveryMode;
        this.RoutingKey = null;
        this.ContentType = null;
        this.ContentEncoding = null;
        this.headers.Clear();
        this.DeliveryMode = DeliveryMode.Persistent;
        this.Priority = default;
        this.CorrelationId = null;
        this.ReplyTo = null;
        this.Expiration = null;
        this.MessageId = null;
        this.Timestamp = default;
        this.Type = null;
        this.UserId = null;
        this.ApplicationId = null;
    }

    public MessageFlags Flags { get; private set; } = MessageFlags.DeliveryMode;

    public string RoutingKey { get; private set; }

    public string ContentType { get; private set; }

    public string ContentEncoding { get; private set; }

    public IReadOnlyDictionary<string, object> Headers
        => (this.Flags & MessageFlags.Headers) == MessageFlags.Headers ? this.headers : null;

    public DeliveryMode DeliveryMode { get; private set; } = DeliveryMode.Persistent;

    public byte Priority { get; private set; }

    public string CorrelationId { get; private set; }

    public string ReplyTo { get; private set; }

    public string Expiration { get; private set; }

    public string MessageId { get; private set; }

    public DateTimeOffset Timestamp { get; private set; }

    public string Type { get; private set; }

    public string UserId { get; private set; }

    public string ApplicationId { get; private set; }

    IMessageBuilder IMessageBuilder.RoutingKey(string value)
    {
        this.RoutingKey = value;
        return this;
    }

    IMessageBuilder IMessageBuilder.ContentType(string value)
    {
        this.ContentType = value;
        this.Flags |= MessageFlags.ContentType;
        return this;
    }

    IMessageBuilder IMessageBuilder.ContentEncoding(string value)
    {
        this.ContentEncoding = value;
        this.Flags |= MessageFlags.ContentEncoding;
        return this;
    }

    public IMessageBuilder SetHeader(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        this.headers[key] = value;
        this.Flags |= MessageFlags.Headers;
        return this;
    }

    IMessageBuilder IMessageBuilder.DeliveryMode(DeliveryMode value)
    {
        this.DeliveryMode = value;
        this.Flags |= MessageFlags.DeliveryMode;
        return this;
    }

    IMessageBuilder IMessageBuilder.Priority(byte value)
    {
        this.Priority = value;
        this.Flags |= MessageFlags.Priority;
        return this;
    }

    IMessageBuilder IMessageBuilder.CorrelationId(string value)
    {
        this.CorrelationId = value;
        this.Flags |= MessageFlags.CorrelationId;
        return this;
    }

    IMessageBuilder IMessageBuilder.ReplyTo(string value)
    {
        this.ReplyTo = value;
        this.Flags |= MessageFlags.ReplyTo;
        return this;
    }

    IMessageBuilder IMessageBuilder.Expiration(string value)
    {
        this.Expiration = value;
        this.Flags |= MessageFlags.Expiration;
        return this;
    }

    IMessageBuilder IMessageBuilder.MessageId(string value)
    {
        this.MessageId = value;
        this.Flags |= MessageFlags.MessageId;
        return this;
    }

    IMessageBuilder IMessageBuilder.Timestamp(DateTimeOffset value)
    {
        this.Timestamp = value;
        this.Flags |= MessageFlags.Timestamp;
        return this;
    }

    IMessageBuilder IMessageBuilder.Type(string value)
    {
        this.Type = value;
        this.Flags |= MessageFlags.Type;
        return this;
    }

    IMessageBuilder IMessageBuilder.UserId(string value)
    {
        this.UserId = value;
        this.Flags |= MessageFlags.UserId;
        return this;
    }

    IMessageBuilder IMessageBuilder.ApplicationId(string value)
    {
        this.ApplicationId = value;
        this.Flags |= MessageFlags.ApplicationId;
        return this;
    }
}