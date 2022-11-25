using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal class DeliveredMessage : IDeliveredMessage, IDisposable
{
    private readonly ISerializer serializer;
    private readonly IPayload payload;
    private readonly string exchange;
    private readonly string routingKey;
    private readonly bool redelivered;
    private bool disposed;

    public DeliveredMessage(ISerializer serializer, DeliverMethod deliverMethod, IPayload payload)
    {
        this.serializer = serializer;
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
        
        var bytes = this.payload.GetBody();
        return this.serializer.Deserialize<T>(this.Properties, bytes);
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