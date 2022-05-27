using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Channels
{
    internal class MethodSender
    {
        private readonly ObjectPool<FrameBuilder> frameBuilderPool;
        private readonly ChannelWriter<MemoryBlock> socketWriter;

        private readonly IMethodFormatter<PublishMethod> publishMethodFormatter;
        private readonly IMethodRegistry registry;
        private readonly ushort channelNumber;
        private readonly int frameMaxSize;

        public MethodSender(ushort channelNumber, ChannelWriter<MemoryBlock> socketWriter, IMethodRegistry registry, ObjectPool<FrameBuilder> frameBuilderPool, int frameMaxSize)
        {
            this.channelNumber = channelNumber;
            this.frameMaxSize = frameMaxSize;
            this.socketWriter = socketWriter;
            this.registry = registry;
            this.publishMethodFormatter = registry.GetFormatter<PublishMethod>();
            this.frameBuilderPool = frameBuilderPool;
        }

        public ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
        {
            var frameBuilder = this.frameBuilderPool.Get();
            frameBuilder.Initialize(this.channelNumber, this.frameMaxSize);
            var formatter = this.registry.GetFormatter<TRequest>();
            frameBuilder.WriteMethodFrame(request, formatter);

            return frameBuilder.WriteToAsync(this.socketWriter, cancellation);
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

            return frameBuilder.WriteToAsync(this.socketWriter, cancellation);
        }
    }
}