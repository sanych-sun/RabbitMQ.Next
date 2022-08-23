using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher;

internal sealed class ReturnMessageHandler : IMessageHandler<ReturnMethod>
{
    private readonly IReadOnlyList<IReturnedMessageHandler> messageHandlers;
    private readonly ISerializerFactory serializerFactory;
    private readonly Channel<ReturnedMessage> returnChannel;

    public ReturnMessageHandler(IReadOnlyList<IReturnedMessageHandler> messageHandlers, ISerializerFactory serializerFactory)
    {
        this.messageHandlers = messageHandlers;
        this.serializerFactory = serializerFactory;
        this.returnChannel = Channel.CreateUnbounded<ReturnedMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false,
        });
            
        Task.Factory.StartNew(this.ProcessReturnedMessagesAsync, TaskCreationOptions.LongRunning);
    }

    public bool Handle(ReturnMethod method, IPayload payload)
    {
        this.returnChannel.Writer.TryWrite(new ReturnedMessage(this.serializerFactory, method, payload));
        return true;
    }

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
                    for (var i = 0; i < this.messageHandlers.Count; i++)
                    {
                        if (await this.messageHandlers[i].TryHandleAsync(returned))
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    returned.Dispose();
                }
            }
        }
    }
}