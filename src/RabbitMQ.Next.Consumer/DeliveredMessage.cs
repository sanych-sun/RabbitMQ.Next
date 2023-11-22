using System;
using System.Collections.Generic;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal class DeliveredMessage : IDeliveredMessage, IContentAccessor
{
    private readonly IPayload payload;
    
    public DeliveredMessage(DeliverMethod deliverMethod, IPayload payload)
    {
        this.payload = payload;
        this.Exchange = deliverMethod.Exchange;
        this.RoutingKey = deliverMethod.RoutingKey;
        this.Redelivered = deliverMethod.Redelivered;
        this.DeliveryTag = deliverMethod.DeliveryTag;
    }

    public void Dispose()
    {
        this.payload.Dispose();
    }

    public T Get<T>() => this.payload.Get<T>();
    
    public string Exchange { get; }

    public string RoutingKey { get; }

    public bool Redelivered { get; }
    
    public ulong DeliveryTag { get; }

    public string ContentType => this.payload.ContentType;

    public string ContentEncoding => this.payload.ContentEncoding;

    public IReadOnlyDictionary<string, object> Headers => this.payload.Headers;

    public DeliveryMode DeliveryMode => this.payload.DeliveryMode;

    public byte Priority => this.payload.Priority;

    public string CorrelationId => this.payload.CorrelationId;

    public string ReplyTo => this.payload.ReplyTo;

    public string Expiration => this.payload.Expiration;

    public string MessageId => this.payload.MessageId;

    public DateTimeOffset Timestamp => this.payload.Timestamp;

    public string Type => this.payload.Type;

    public string UserId => this.payload.UserId;

    public string ApplicationId => this.payload.ApplicationId;
}