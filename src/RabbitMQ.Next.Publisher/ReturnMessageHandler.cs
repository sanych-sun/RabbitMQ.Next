using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher;

internal sealed class ReturnMessageHandler : IMessageHandler<ReturnMethod>
{
    private readonly Func<IReturnedMessage,Task> messageHandler;
    private readonly ISerializer serializer;
    private readonly Channel<ReturnedMessage> returnChannel;

    public ReturnMessageHandler(Func<IReturnedMessage,Task> messageHandler, ISerializer serializer)
    {
        this.messageHandler = messageHandler;
        this.serializer = serializer;
        this.returnChannel = Channel.CreateUnbounded<ReturnedMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false,
        });
            
        Task.Factory.StartNew(this.ProcessReturnedMessagesAsync, TaskCreationOptions.LongRunning);
    }

    public void Handle(ReturnMethod method, IPayload payload) 
        => this.returnChannel.Writer.TryWrite(new ReturnedMessage(this.serializer, method, payload));

    public void Release(Exception ex = null)
    {
        this.returnChannel.Writer.TryComplete();
    }

    private async Task ProcessReturnedMessagesAsync()
    {
        var reader = this.returnChannel.Reader;
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var returned))
            {
                try
                {
                    await this.messageHandler(returned); 
                }
                finally
                {
                    returned.Dispose();
                }
            }
        }
    }
}