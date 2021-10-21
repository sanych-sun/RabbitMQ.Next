using System;
using System.Buffers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Channels
{
    internal class MethodSender
    {
        private readonly ObjectPool<FrameBuilder> frameBuilderPool;
        private readonly ChannelWriter<IMemoryOwner<byte>> socketWriter;
        private readonly SemaphoreSlim senderSync;

        private readonly IMethodFormatter<PublishMethod> publishMethodFormatter;
        private readonly IMethodRegistry registry;

        public MethodSender(ChannelWriter<IMemoryOwner<byte>> socketWriter, IMethodRegistry registry, ObjectPool<FrameBuilder> frameBuilderPool)
        {
            this.socketWriter = socketWriter;
            this.registry = registry;
            this.publishMethodFormatter = registry.GetFormatter<PublishMethod>();
            this.frameBuilderPool = frameBuilderPool;
            this.senderSync = new SemaphoreSlim(1,1);
        }

        public ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
        {
            var frameBuilder = this.frameBuilderPool.Get();
            var buffer = frameBuilder.BeginMethodFrame(request.MethodId);

            var formatter = this.registry.GetFormatter<TRequest>();

            var written = formatter.Write(buffer.GetMemory(), request);
            buffer.Advance(written);
            frameBuilder.EndFrame();

            return this.TransmitFrameAsync(frameBuilder, cancellation);
        }

        public ValueTask PublishAsync<TState>(
            TState state, string exchange, string routingKey,
            IMessageProperties properties, Action<TState, IBufferWriter<byte>> payload,
            PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default)
        {
            var frameBuilder = this.frameBuilderPool.Get();

            var buffer = frameBuilder.BeginMethodFrame(MethodId.BasicPublish);

            var written = this.publishMethodFormatter.Write(buffer.GetMemory(), new PublishMethod(exchange, routingKey, (byte)flags));
            buffer.Advance(written);
            frameBuilder.EndFrame();

            var contentBuffer = frameBuilder.BeginContentFrame(properties);
            payload(state, contentBuffer);
            frameBuilder.EndFrame();

            return this.TransmitFrameAsync(frameBuilder, cancellation);
        }

        private async ValueTask TransmitFrameAsync(FrameBuilder frame, CancellationToken cancellation)
        {
            await this.senderSync.WaitAsync(cancellation);

            try
            {
                await frame.WriteToAsync(this.socketWriter);
            }
            finally
            {
                this.senderSync.Release();
                this.frameBuilderPool.Return(frame);
            }
        }
    }
}