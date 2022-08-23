using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal class DeliveredMessage : IDeliveredMessage, IDisposable
{
    private readonly ISerializerFactory serializerFactory;
    private readonly IPayload payload;
    private readonly string exchange;
    private readonly string routingKey;
    private readonly bool redelivered;
    private bool disposed;

    public DeliveredMessage(ISerializerFactory serializerFactory, DeliverMethod deliverMethod, IPayload payload)
    {
        this.serializerFactory = serializerFactory;
        this.payload = payload;
        this.exchange = deliverMethod.Exchange;
        this.routingKey = deliverMethod.RoutingKey;
        this.redelivered = deliverMethod.Redelivered;
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

    public bool Redelivered
    {
        get
        {
            this.ValidateState();
            return this.redelivered;
        }
    }

    public IMessageProperties Properties
    {
        get
        {
            this.ValidateState();
            return this.payload;
        }
    }

    public T Content<T>()
    {
        this.ValidateState();
        
        var serializer = this.serializerFactory.Get(this.Properties);
        var bytes = this.payload.GetBody();

        return serializer.Deserialize<T>(bytes);
    }

    public void Dispose()
    {
        this.disposed = true;
        this.payload?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateState()
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(nameof(DeliveredMessage));
        }
    }
}