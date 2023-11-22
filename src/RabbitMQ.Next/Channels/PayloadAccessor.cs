using System;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels;

internal class PayloadAccessor: IPayload
{
    private readonly ISerializer serializer;
    private readonly ObjectPool<LazyMessageProperties> propertiesPool;
    private LazyMessageProperties properties;
    private IMemoryAccessor header;
    private IMemoryAccessor body;

    public PayloadAccessor(ISerializer serializer, ObjectPool<LazyMessageProperties> propertiesPool, IMemoryAccessor header, IMemoryAccessor body)
    {
        this.serializer = serializer;
        this.propertiesPool = propertiesPool;
        this.header = header;
        this.body = body;
    }

    public void Dispose()
    {
        if (this.properties != null)
        {
            this.propertiesPool.Return(this.properties);
            this.properties = null;
        }

        if (this.header != null)
        {
            this.header.Dispose();
            this.header = null;
        }

        if (this.body != null)
        {
            this.body.Dispose();
            this.body = null;
        }
    }

    public T Get<T>()
    {
        if (this.body == null)
        {
            throw new ObjectDisposedException(nameof(PayloadAccessor));
        }

        return this.serializer.Deserialize<T>(this, this.body.ToSequence());
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

    private LazyMessageProperties Properties
    {
        get
        {
            if (this.body == null)
            {
                throw new ObjectDisposedException(nameof(PayloadAccessor));
            }

            if (this.properties == null)
            {
                this.properties = this.propertiesPool.Get();
                this.properties.Set(this.header.Memory);
            }

            return this.properties;
        }
    }
}