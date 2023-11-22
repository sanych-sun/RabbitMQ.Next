using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer;

internal sealed class DeliverMessageHandler : IMessageHandler<DeliverMethod>
{
    private readonly Func<IDeliveredMessage, IContentAccessor, Task> messageHandler;
    private readonly IAcknowledgement acknowledgement;
    private readonly PoisonMessageMode onPoisonMessage;
    private readonly Channel<DeliveredMessage> deliverChannel;

    public DeliverMessageHandler(
        Func<IDeliveredMessage, IContentAccessor, Task> messageHandler,
        IAcknowledgement acknowledgement,
        PoisonMessageMode onPoisonMessage,
        byte concurrencyLevel)
    {
        this.acknowledgement = acknowledgement;
        this.messageHandler = messageHandler;
        this.onPoisonMessage = onPoisonMessage;
            
        this.deliverChannel = Channel.CreateUnbounded<DeliveredMessage>(new UnboundedChannelOptions
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
        => this.deliverChannel.Writer.TryWrite(new (method, payload));


    public void Release(Exception ex = null)
    {
        this.deliverChannel.Writer.TryComplete();
    }

    private async Task ProcessDeliveredMessagesAsync()
    {
        var reader = this.deliverChannel.Reader;
        while (await reader.WaitToReadAsync().ConfigureAwait(false))
        {
            while (reader.TryRead(out var message))
            {
                try
                {
                    await this.messageHandler(message, message).ConfigureAwait(false);
                    await this.acknowledgement.AckAsync(message.DeliveryTag).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await this.acknowledgement.NackAsync(message.DeliveryTag, this.onPoisonMessage == PoisonMessageMode.Requeue).ConfigureAwait(false);
                }
                finally
                {
                    message.Dispose();
                }
            }
        }
    }
}