using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Channels
{
    internal class MethodSender
    {
        private readonly ObjectPool<FrameBuilder> frameBuilderPool;

        private readonly IConnectionInternal connection;
        private readonly IMethodFormatter<PublishMethod> publishMethodFormatter;
        private readonly IMethodRegistry registry;
        private readonly ushort channelNumber;
        private readonly int frameMaxSize;

        public MethodSender(ushort channelNumber, IConnectionInternal connection, int frameMaxSize)
        {
            this.channelNumber = channelNumber;
            this.frameMaxSize = frameMaxSize;
            this.connection = connection;
            this.registry = connection.MethodRegistry;
            this.publishMethodFormatter = registry.GetFormatter<PublishMethod>();
            this.frameBuilderPool = connection.FrameBuilderPool;
        }

        public ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
        {
            var frameBuilder = this.frameBuilderPool.Get();
            frameBuilder.Initialize(this.channelNumber, this.frameMaxSize);
            var formatter = this.registry.GetFormatter<TRequest>();
            frameBuilder.WriteMethodFrame(request, formatter);

            return this.connection.WriteToSocketAsync(frameBuilder.Complete(), cancellation);
        }

        public ValueTask PublishAsync<TState>(
            TState state, string exchange, string routingKey,
            IMessageProperties properties, Action<TState, IBufferWriter<byte>> contentBody,
            PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default)
        {
            var frameBuilder = this.frameBuilderPool.Get();
            frameBuilder.Initialize(this.channelNumber, this.frameMaxSize);
            var publishMethod = new PublishMethod(exchange, routingKey, (byte)flags);

            frameBuilder.WriteMethodFrame(publishMethod, this.publishMethodFormatter);
            frameBuilder.WriteContentFrame(state, properties, contentBody);

            return this.connection.WriteToSocketAsync(frameBuilder.Complete(), cancellation);
        }
    }
}