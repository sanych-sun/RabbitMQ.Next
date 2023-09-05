using System;
using System.Buffers;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Transport;

internal class PayloadAccessor: IPayload
{
    private readonly ObjectPool<LazyMessageProperties> propertiesPool;
    private LazyMessageProperties properties;
    private IMemoryAccessor header;
    private IMemoryAccessor body;

    public PayloadAccessor(ObjectPool<LazyMessageProperties> propertiesPool, IMemoryAccessor header, IMemoryAccessor body)
    {
        this.propertiesPool = propertiesPool;
        this.header = header;
        this.body = body;
            
        this.properties = propertiesPool.Get();
        this.properties.Set(header.Memory);
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

    public ReadOnlySequence<byte> GetBody()
    {
        if (this.body == null)
        {
            throw new ObjectDisposedException(nameof(PayloadAccessor));
        }

        return this.body.ToSequence();
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
            if (this.properties == null)
            {
                throw new ObjectDisposedException(nameof(PayloadAccessor));
            }

            return this.properties;
        }
    }
}