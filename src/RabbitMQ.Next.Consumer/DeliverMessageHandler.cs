using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal sealed class DeliverMessageHandler : IMessageHandler<DeliverMethod>
{
    private readonly IReadOnlyList<IDeliveredMessageHandler> messageHandlers;
    private readonly ISerializerFactory serializerFactory;
    private readonly IAcknowledgement acknowledgement;
    private readonly UnprocessedMessageMode onUnprocessedMessage;
    private readonly UnprocessedMessageMode onPoisonMessage;
    private readonly Channel<(DeliveredMessage message, ulong deliveryTag)> deliverChannel;

    public DeliverMessageHandler(
        IAcknowledgement acknowledgement,
        IReadOnlyList<IDeliveredMessageHandler> messageHandlers,
        ISerializerFactory serializerFactory,
        UnprocessedMessageMode onUnprocessedMessage,
        UnprocessedMessageMode onPoisonMessage,
        byte concurrencyLevel)
    {
        this.acknowledgement = acknowledgement;
        this.messageHandlers = messageHandlers;
        this.serializerFactory = serializerFactory;
        this.onUnprocessedMessage = onUnprocessedMessage;
        this.onPoisonMessage = onPoisonMessage;
            
        this.deliverChannel = Channel.CreateUnbounded<(DeliveredMessage message, ulong deliveryTag)>(new UnboundedChannelOptions
        {
            SingleWriter = true,
            SingleReader = concurrencyLevel == 1,
            AllowSynchronousContinuations = false
        });


        for (var i = 0; i < concurrencyLevel; i++)
        {
            Task.Factory.StartNew(this.ProcessDeliveredMessagesAsync, TaskCreationOptions.LongRunning);    
        }
    }

    public bool Handle(DeliverMethod method, IPayload payload)
    {
        this.deliverChannel.Writer.TryWrite((new DeliveredMessage(this.serializerFactory, method, payload), method.DeliveryTag));
        return true;
    }
        

    public void Release(Exception ex = null)
    {
        this.deliverChannel.Writer.TryComplete();
    }

    private async Task ProcessDeliveredMessagesAsync()
    {
        var reader = this.deliverChannel.Reader;
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var delivered))
            {
                await HandleMessageAsync(delivered.message, delivered.deliveryTag);
            }
        }
    }

    private async ValueTask HandleMessageAsync(DeliveredMessage message, ulong deliveryTag)
    {
        try
        {
            for (var i = 0; i < this.messageHandlers.Count; i++)
            {
                if (await this.messageHandlers[i].TryHandleAsync(message))
                {
                    await this.acknowledgement.AckAsync(deliveryTag);
                    return;
                }
            }
                        
            await this.acknowledgement.NackAsync(deliveryTag, this.onUnprocessedMessage == UnprocessedMessageMode.Requeue);
        }
        catch (Exception)
        {
            await this.acknowledgement.NackAsync(deliveryTag, this.onPoisonMessage == UnprocessedMessageMode.Requeue);
        }
        finally
        {
            message.Dispose();
        }
    }
}