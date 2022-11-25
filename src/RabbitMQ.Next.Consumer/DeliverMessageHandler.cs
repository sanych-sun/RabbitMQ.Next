using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal sealed class DeliverMessageHandler : IMessageHandler<DeliverMethod>
{
    private readonly Func<IDeliveredMessage, Task> messageHandler;
    private readonly ISerializer serializer;
    private readonly IAcknowledgement acknowledgement;
    private readonly PoisonMessageMode onPoisonMessage;
    private readonly Channel<(DeliveredMessage message, ulong deliveryTag)> deliverChannel;

    public DeliverMessageHandler(
        Func<IDeliveredMessage, Task> messageHandler,
        IAcknowledgement acknowledgement,
        ISerializer serializer,
        PoisonMessageMode onPoisonMessage,
        byte concurrencyLevel)
    {
        this.acknowledgement = acknowledgement;
        this.messageHandler = messageHandler;
        this.serializer = serializer;
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
        this.deliverChannel.Writer.TryWrite((new DeliveredMessage(this.serializer, method, payload), method.DeliveryTag));
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
                try
                {
                    await this.messageHandler(delivered.message);
                    await this.acknowledgement.AckAsync(delivered.deliveryTag);
                }
                catch (Exception)
                {
                    await this.acknowledgement.NackAsync(delivered.deliveryTag, this.onPoisonMessage == PoisonMessageMode.Requeue);
                }
                finally
                {
                    delivered.message.Dispose();
                }
            }
        }
    }
}