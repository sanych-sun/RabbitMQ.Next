using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Connection;

namespace RabbitMQ.Next.Channels
{
    internal class ConnectionCloseHandler : IFrameHandler
    {
        private readonly IChannelInternal channel;
        private readonly IMethodParser<CloseMethod> closeMethodParser;

        public ConnectionCloseHandler(IChannelInternal channel, IMethodRegistry registry)
        {
            this.channel = channel;
            this.closeMethodParser = registry.GetParser<CloseMethod>();
        }

        public ValueTask<bool> HandleMethodFrameAsync(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId == MethodId.ConnectionClose)
            {
                var closeMethod = this.closeMethodParser.Parse(payload);
                this.channel.SetCompleted(new ConnectionException(closeMethod.StatusCode, closeMethod.Description));
                return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(false);
        }

        public ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
            => new(false);
    }
}