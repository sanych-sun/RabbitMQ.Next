using System;
using System.Collections.Generic;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

internal class MessageBuilder : IMessageBuilder
{
    private readonly Dictionary<string, object> headers = new();

    public MessageBuilder(string exchange)
    {
        if (string.IsNullOrEmpty(exchange))
        {
            throw new ArgumentNullException(nameof(exchange));
        }
        
        this.Exchange = exchange;
    }
    
    public void Reset()
    {
        this.ClrType = null;
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
        this.Mandatory = false;
        this.Immediate = false;
    }
    
    public bool Mandatory { get; private set; }
    
    public bool Immediate { get; private set; }

    public Type ClrType { get; private set; }
    
    public string Exchange { get; private set; }

    public string RoutingKey { get; private set; }

    public string ContentType { get; private set; }

    public string ContentEncoding { get; private set; }

    public IReadOnlyDictionary<string, object> Headers
        => this.headers.Count == 0 ? null : this.headers;

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
    
    public IMessageBuilder SetMandatory()
    {
        this.Mandatory = true;
        return this;
    }

    public IMessageBuilder SetImmediate()
    {
        this.Immediate = true;
        return this;
    }

    public void SetClrType(Type crlType)
    {
        this.ClrType = crlType;
    }
    
    public IMessageBuilder SetRoutingKey(string value)
    {
        this.RoutingKey = value;
        return this;
    }

    public IMessageBuilder SetContentType(string value)
    {
        this.ContentType = value;
        return this;
    }

    public IMessageBuilder SetContentEncoding(string value)
    {
        this.ContentEncoding = value;
        return this;
    }

    public IMessageBuilder SetHeader(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        this.headers[key] = value;
        return this;
    }

    public IMessageBuilder SetDeliveryMode(DeliveryMode value)
    {
        this.DeliveryMode = value;
        return this;
    }

    public IMessageBuilder SetPriority(byte value)
    {
        this.Priority = value;
        return this;
    }

    public IMessageBuilder SetCorrelationId(string value)
    {
        this.CorrelationId = value;
        return this;
    }

    public IMessageBuilder SetReplyTo(string value)
    {
        this.ReplyTo = value;
        return this;
    }

    public IMessageBuilder SetExpiration(string value)
    {
        this.Expiration = value;
        return this;
    }

    public IMessageBuilder SetMessageId(string value)
    {
        this.MessageId = value;
        return this;
    }

    public IMessageBuilder SetTimestamp(DateTimeOffset value)
    {
        this.Timestamp = value;
        return this;
    }

    public IMessageBuilder SetType(string value)
    {
        this.Type = value;
        return this;
    }

    public IMessageBuilder SetUserId(string value)
    {
        this.UserId = value;
        return this;
    }

    public IMessageBuilder SetApplicationId(string value)
    {
        this.ApplicationId = value;
        return this;
    }
}