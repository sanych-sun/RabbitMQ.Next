using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Channels
{
    internal class ChannelCloseHandler : IFrameHandler
    {
        private readonly IChannelInternal channel;
        private readonly IMethodParser<CloseMethod> closeMethodParser;

        public ChannelCloseHandler(IChannelInternal channel, IMethodRegistry registry)
        {
            this.channel = channel;
            this.closeMethodParser = registry.GetParser<CloseMethod>();
        }
        
        public ValueTask<bool> HandleMethodFrameAsync(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId == MethodId.ChannelClose)
            {
                var closeMethod = this.closeMethodParser.Parse(payload);
                return this.HandleInternalAsync(closeMethod);
            }

            return new ValueTask<bool>(false);
        }

        public ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
            => new(false);

        private async ValueTask<bool> HandleInternalAsync(CloseMethod closeMethod)
        {
            await this.channel.SendAsync(new CloseOkMethod());

            this.channel.SetCompleted(new ChannelException(closeMethod.StatusCode, closeMethod.Description, closeMethod.FailedMethodId));

            return true;
        }
    }
}