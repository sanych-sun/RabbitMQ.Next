using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class ReturnMessageHandler : IMessageHandler<ReturnMethod>
    {
        private readonly IReadOnlyList<IReturnedMessageHandler> messageHandlers;
        private readonly Channel<(ReturnedMessage message, IContent content)> returnChannel;

        public ReturnMessageHandler(IReadOnlyList<IReturnedMessageHandler> messageHandlers)
        {
            this.messageHandlers = messageHandlers;
            this.returnChannel = Channel.CreateUnbounded<(ReturnedMessage message, IContent content)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false,
            });
            
            Task.Factory.StartNew(this.ProcessReturnedMessagesAsync, TaskCreationOptions.LongRunning);
        }

        public bool Handle(ReturnMethod method, IContent content)
        {
            this.returnChannel.Writer.TryWrite((
                new ReturnedMessage(method.Exchange, method.RoutingKey, method.ReplyCode, method.ReplyText),
                content));
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
                var returned = await reader.ReadAsync();

                try
                {
                    for (var i = 0; i < this.messageHandlers.Count; i++)
                    {
                        if (await this.messageHandlers[i].TryHandleAsync(returned.message, returned.content))
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    returned.content.Dispose();
                }
            }
        }
    }
}