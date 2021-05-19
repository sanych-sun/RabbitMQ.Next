using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Buffers;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Sockets;

namespace RabbitMQ.Next.Transport
{
    internal class FrameSender : IFrameSender
    {
        private static readonly byte[] FrameEndPayload = { ProtocolConstants.FrameEndByte };
        private static readonly ReadOnlyMemory<byte> HeartbeatFrame = new byte[] { (byte)FrameType.Heartbeat, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        private readonly ISocket socket;
        private readonly IMethodRegistry registry;
        private readonly IBufferPool bufferPool;

        public FrameSender(ISocket socket, IMethodRegistry registry, IBufferPool bufferPool)
        {
            this.socket = socket;
            this.registry = registry;
            this.bufferPool = bufferPool;

            this.FrameMaxSize = ProtocolConstants.FrameMinSize;
        }

        public int FrameMaxSize { get; set; }

        public ValueTask SendHeartBeatAsync()
            => this.socket.SendAsync((0), async (sender, _) =>
            {
                await sender(HeartbeatFrame);
                await sender(FrameEndPayload);
            });

        public async ValueTask SendMethodAsync<TMethod>(ushort channelNumber, TMethod method)
            where TMethod : struct, IOutgoingMethod
        {
            using var headerBuffer = this.bufferPool.CreateMemory(ProtocolConstants.FrameHeaderSize);
            using var payloadBuffer = this.bufferPool.CreateMemory();

            var written = this.registry.FormatMessage(method, payloadBuffer.Memory);
            var payload = payloadBuffer.Memory.Slice(0, written);
            headerBuffer.Memory.Span.WriteFrameHeader(FrameType.Method, channelNumber, (uint)written);

            await this.socket.SendAsync(
                (header: headerBuffer.Memory, payload),
                async (sender, state) =>
                {
                    await sender(state.header);
                    await sender(state.payload);
                    await sender(FrameEndPayload);
                });
        }

        public async ValueTask SendContentHeaderAsync(ushort channelNumber, IMessageProperties properties, ulong contentSize)
        {
            using var headerBuffer = this.bufferPool.CreateMemory(ProtocolConstants.FrameHeaderSize);
            using var payloadBuffer = this.bufferPool.CreateMemory();

            var written = payloadBuffer.Memory.Span.WriteContentHeader(properties, contentSize);
            var payload = payloadBuffer.Memory.Slice(0, written);
            headerBuffer.Memory.Span.WriteFrameHeader(FrameType.ContentHeader, channelNumber, (uint)written);

            await this.socket.SendAsync(
                (header: headerBuffer.Memory, payload),
                async (sender, state) =>
                {
                    await sender(state.header);
                    await sender(state.payload);
                    await sender(FrameEndPayload);
                });
        }

        public async ValueTask SendContentAsync(ushort channelNumber, ReadOnlySequence<byte> contentBytes)
        {
            using var frameHeaderBuffer = this.bufferPool.CreateMemory(ProtocolConstants.FrameHeaderSize);

            if (contentBytes.IsSingleSegment)
            {
                await this.SendContentChunkAsync(frameHeaderBuffer.Memory, channelNumber, contentBytes.First);
            }
            else
            {
                var enumerator = new SequenceEnumerator<byte>(contentBytes);
                while (enumerator.MoveNext())
                {
                    await this.SendContentChunkAsync(frameHeaderBuffer.Memory, channelNumber, enumerator.Current);
                }
            }
        }

        private async ValueTask SendContentChunkAsync(Memory<byte> headerBuffer, ushort channelNumber, ReadOnlyMemory<byte> content)
        {
            while (content.Length > 0)
            {
                var chunk = content;
                if (chunk.Length > this.FrameMaxSize)
                {
                    chunk = content.Slice(0, this.FrameMaxSize);
                }

                headerBuffer.Span.WriteFrameHeader(FrameType.ContentBody, channelNumber, (uint)chunk.Length);

                await this.socket.SendAsync(( headerBuffer, chunk), async (sender, state) =>
                {
                    await sender(state.headerBuffer);
                    await sender(state.chunk);
                    await sender(FrameEndPayload);
                });

                content = content.Slice(chunk.Length);
            }
        }
    }
}