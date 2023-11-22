using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher;

internal sealed class ReturnMessageHandler : IMessageHandler<ReturnMethod>
{
    private readonly Func<IReturnedMessage, IContentAccessor, Task> pipeline;
    private readonly Channel<ReturnedMessage> returnChannel;

    public ReturnMessageHandler(IReadOnlyList<Func<IReturnedMessage,IContentAccessor,Func<IReturnedMessage,IContentAccessor,Task>,Task>> middlewares)
    {
        if (middlewares != null && middlewares.Count > 0)
        {
            this.pipeline = (_, _) => default;
            for (var i = middlewares.Count - 1; i >= 0; i--)
            {
                var next = this.pipeline;
                var handler = middlewares[i];
                this.pipeline = (m, c) => handler.Invoke(m, c, next);
            }
            
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
        this.returnChannel?.Writer.TryWrite(new ReturnedMessage(method, payload));
    }

    public void Release(Exception ex = null)
    {
        this.returnChannel?.Writer.TryComplete(ex);
    }

    private async Task ProcessReturnedMessagesAsync()
    {
        var reader = this.returnChannel.Reader;
        while (await reader.WaitToReadAsync().ConfigureAwait(false))
        {
            while (reader.TryRead(out var message))
            {
                try
                {
                    await this.pipeline.Invoke(message, message).ConfigureAwait(false); 
                }
                finally
                {
                    message.Dispose();
                }
            }
        }
    }
}