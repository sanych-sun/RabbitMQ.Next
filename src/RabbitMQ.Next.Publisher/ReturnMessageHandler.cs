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
    private readonly IReturnMiddleware returnPipeline;
    private readonly ISerializer serializer;
    private readonly Channel<ReturnedMessage> returnChannel;

    public ReturnMessageHandler(IReturnMiddleware returnPipeline, ISerializer serializer)
    {
        this.returnPipeline = returnPipeline;
        this.serializer = serializer;

        if (this.returnPipeline != null)
        {
            this.returnChannel = Channel.CreateUnbounded<ReturnedMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false,
            });
            
            Task.Factory.StartNew(this.ProcessReturnedMessagesAsync, TaskCreationOptions.LongRunning);
        }
    }

    public void Handle(ReturnMethod method, IPayload payload)
    {
        if (this.returnPipeline == null)
        {
            return;
        }
        
        this.returnChannel.Writer.TryWrite(new ReturnedMessage(this.serializer, method, payload));
    }

    public void Release(Exception ex = null)
    {
        this.returnChannel?.Writer.TryComplete();
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
                    await this.returnPipeline.InvokeAsync(returned, default); 
                }
                finally
                {
                    returned.Dispose();
                }
            }
        }
    }
}