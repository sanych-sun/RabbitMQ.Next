using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal sealed class DeliverMessageHandler : IMessageHandler<DeliverMethod>
{
    private readonly IReadOnlyList<IDeliveredMessageHandler> messageHandlers;
    private readonly IAcknowledgement acknowledgement;
    private readonly UnprocessedMessageMode onUnprocessedMessage;
    private readonly UnprocessedMessageMode onPoisonMessage;
    private readonly Channel<(DeliveredMessage message, IContent content)> deliverChannel;

    public DeliverMessageHandler(
        IAcknowledgement acknowledgement,
        IReadOnlyList<IDeliveredMessageHandler> messageHandlers,
        UnprocessedMessageMode onUnprocessedMessage,
        UnprocessedMessageMode onPoisonMessage,
        byte concurrencyLevel)
    {
        this.acknowledgement = acknowledgement;
        this.messageHandlers = messageHandlers;
        this.onUnprocessedMessage = onUnprocessedMessage;
        this.onPoisonMessage = onPoisonMessage;
            
        this.deliverChannel = Channel.CreateUnbounded<(DeliveredMessage message, IContent content)>(new UnboundedChannelOptions
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

    public bool Handle(DeliverMethod method, IContent content)
    {
        this.deliverChannel.Writer.TryWrite((
            new DeliveredMessage(method.Exchange, method.RoutingKey, method.Redelivered, method.ConsumerTag, method.DeliveryTag),
            content));
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
                await this.HandleMessageAsync(delivered.message, delivered.content);
            }
        }
    }

    private async ValueTask HandleMessageAsync(DeliveredMessage message, IContent content)
    {
        try
        {
            for (var i = 0; i < this.messageHandlers.Count; i++)
            {
                if (await this.messageHandlers[i].TryHandleAsync(message, content))
                {
                    await this.acknowledgement.AckAsync(message.DeliveryTag);
                    return;
                }
            }
                        
            await this.acknowledgement.NackAsync(message.DeliveryTag, this.onUnprocessedMessage == UnprocessedMessageMode.Requeue);
        }
        catch (Exception)
        {
            await this.acknowledgement.NackAsync(message.DeliveryTag, this.onPoisonMessage == UnprocessedMessageMode.Requeue);
        }
        finally
        {
            content.Dispose();
        }
    }
}