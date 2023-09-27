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
    private readonly Func<IDeliveredMessage, ValueTask> messageHandler;
    private readonly ISerializer serializer;
    private readonly IAcknowledgement acknowledgement;
    private readonly PoisonMessageMode onPoisonMessage;
    private readonly Channel<(DeliverMethod method, IPayload payload)> deliverChannel;

    public DeliverMessageHandler(
        Func<IDeliveredMessage, ValueTask> messageHandler,
        IAcknowledgement acknowledgement,
        ISerializer serializer,
        PoisonMessageMode onPoisonMessage,
        byte concurrencyLevel)
    {
        this.acknowledgement = acknowledgement;
        this.messageHandler = messageHandler;
        this.serializer = serializer;
        this.onPoisonMessage = onPoisonMessage;
            
        this.deliverChannel = Channel.CreateUnbounded<(DeliverMethod method, IPayload payload)>(new UnboundedChannelOptions
        {
            SingleWriter = true,
            SingleReader = concurrencyLevel == 1,
            AllowSynchronousContinuations = false,
        });


        for (var i = 0; i < concurrencyLevel; i++)
        {
            Task.Factory.StartNew(this.ProcessDeliveredMessagesAsync, TaskCreationOptions.LongRunning);    
        }
    }

    public void Handle(DeliverMethod method, IPayload payload) 
        => this.deliverChannel.Writer.TryWrite((method, payload));


    public void Release(Exception ex = null)
    {
        this.deliverChannel.Writer.TryComplete();
    }

    private async Task ProcessDeliveredMessagesAsync()
    {
        var reader = this.deliverChannel.Reader;
        while (await reader.WaitToReadAsync().ConfigureAwait(false))
        {
            while (reader.TryRead(out var delivered))
            {
                var message = new DeliveredMessage(this.serializer, delivered.method, delivered.payload);
                try
                {
                    await this.messageHandler(message).ConfigureAwait(false);
                    await this.acknowledgement.AckAsync(delivered.method.DeliveryTag).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await this.acknowledgement.NackAsync(delivered.method.DeliveryTag, this.onPoisonMessage == PoisonMessageMode.Requeue).ConfigureAwait(false);
                }
                finally
                {
                    message.Dispose();
                }
            }
        }
    }
}