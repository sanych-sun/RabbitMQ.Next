using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher;

internal class ReturnedMessage : IReturnedMessage, IDisposable
{
    private readonly ISerializer serializer;
    private readonly IPayload payload;
    private readonly string exchange;
    private readonly string routingKey;
    private readonly ushort replyCode;
    private readonly string replyText;

    private bool disposed;

    public ReturnedMessage(ISerializer serializer, ReturnMethod returnMethod, IPayload payload)
    {
        this.serializer = serializer;
        this.payload = payload;
        this.exchange = returnMethod.Exchange;
        this.routingKey = returnMethod.RoutingKey;
        this.replyCode = returnMethod.ReplyCode;
        this.replyText = returnMethod.ReplyText;
    }


    public string Exchange
    {
        get
        {
            this.ValidateState();
            return this.exchange;
        }
    }

    public string RoutingKey
    {
        get
        {
            this.ValidateState();
            return this.routingKey;
        }
    }

    public ushort ReplyCode
    {
        get
        {
            this.ValidateState();
            return this.replyCode;
        }
    }

    public string ReplyText
    {
        get
        {
            this.ValidateState();
            return this.replyText;
        }
    }

    public string ContentType => this.Properties.ContentType;

    public string ContentEncoding => this.Properties.ContentEncoding;

    public IReadOnlyDictionary<string, object> Headers => this.Properties.Headers;

    public DeliveryMode DeliveryMode => this.Properties.DeliveryMode;

    public byte Priority => this.Properties.Priority;

    public string CorrelationId => this.Properties.CorrelationId;

    public string ReplyTo => this.Properties.ReplyTo;

    public string Expiration => this.Properties.Expiration;

    public string MessageId => this.Properties.MessageId;

    public DateTimeOffset Timestamp => this.Properties.Timestamp;

    public string Type => this.Properties.Type;

    public string UserId => this.Properties.UserId;

    public string ApplicationId => this.Properties.ApplicationId;

    public T Content<T>()
    {
        this.ValidateState();
        
        var bytes = this.payload.GetBody();
        return this.serializer.Deserialize<T>(this.payload, bytes);
    }

    public void Dispose()
    {
        this.disposed = true;
        this.payload?.Dispose();
    }

    private IMessageProperties Properties
    {
        get
        {
            this.ValidateState();
            return this.payload;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateState()
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(nameof(ReturnedMessage));
        }
    }
}