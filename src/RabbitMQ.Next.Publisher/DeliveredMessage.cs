using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher;

internal class ReturnedMessage : IReturnedMessage, IDisposable
{
    private readonly ISerializerFactory serializerFactory;
    private readonly IPayload payload;
    private readonly string exchange;
    private readonly string routingKey;
    private readonly ushort replyCode;
    private readonly string replyText;
    private bool disposed;

    public ReturnedMessage(ISerializerFactory serializerFactory, ReturnMethod returnMethod, IPayload payload)
    {
        this.serializerFactory = serializerFactory;
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
            throw new ObjectDisposedException(nameof(ReturnedMessage));
        }
    }
}