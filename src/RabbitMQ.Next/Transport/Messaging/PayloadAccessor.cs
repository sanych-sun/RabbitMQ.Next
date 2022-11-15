using System;
using System.Buffers;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Transport.Messaging;

internal class PayloadAccessor: IPayload
{
    private readonly ObjectPool<LazyMessageProperties> propertiesPool;
    private readonly ObjectPool<MemoryBlock> memoryPool;
    private LazyMessageProperties properties;
    private MemoryBlock header;
    private MemoryBlock body;

    public PayloadAccessor(ObjectPool<LazyMessageProperties> propertiesPool, ObjectPool<MemoryBlock> memoryPool, MemoryBlock header, MemoryBlock body)
    {
        this.propertiesPool = propertiesPool;
        this.memoryPool = memoryPool;
        this.header = header;
        this.body = body;
            
        this.properties = propertiesPool.Get();
        this.properties.Set(header.Memory[12..]); // 2 obsolete shorts + ulong
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
            this.memoryPool.Return(this.header);
            this.header = null;
        }

        if (this.body != null)
        {
            this.memoryPool.Return(this.body);
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

    public MessageFlags Flags => this.Properties.Flags;
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