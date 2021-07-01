using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Sockets;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport
{
    internal class FrameSender : IFrameSender
    {
        private static readonly byte[] FrameEndPayload = { ProtocolConstants.FrameEndByte };
        private static readonly ReadOnlyMemory<byte> HeartbeatFrame = new byte[] { (byte)FrameType.Heartbeat, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, ProtocolConstants.FrameEndByte };
        private static readonly ReadOnlyMemory<byte> AmqpHeader = new byte[] { 0x41, 0x4D, 0x51, 0x50, 0x00, 0x00, 0x09, 0x01 };

        private readonly ISocket socket;
        private readonly IMethodRegistry registry;
        private readonly IBufferPool bufferPool;

        public FrameSender(ISocket socket, IMethodRegistry registry, IBufferPool bufferPool)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (bufferPool == null)
            {
                throw new ArgumentNullException(nameof(bufferPool));
            }

            this.socket = socket;
            this.registry = registry;
            this.bufferPool = bufferPool;

            this.FrameMaxSize = ProtocolConstants.FrameMinSize;
        }

        public int FrameMaxSize { get; set; }

        public ValueTask SendHeartBeatAsync()
            => this.socket.SendAsync(HeartbeatFrame);

        public ValueTask SendAmqpHeaderAsync()
            => this.socket.SendAsync(AmqpHeader);

        public async ValueTask SendMethodAsync<TMethod>(ushort channelNumber, TMethod method)
            where TMethod : struct, IOutgoingMethod
        {
            using var buffer = this.bufferPool.CreateMemory();

            var written = this.registry.FormatMessage(method, buffer.Memory.Slice(ProtocolConstants.FrameHeaderSize));
            buffer.Memory.Span.WriteFrameHeader(FrameType.Method, channelNumber, (uint)written);
            FrameEndPayload.CopyTo(buffer.Memory.Slice(ProtocolConstants.FrameHeaderSize + written));

            var payload = buffer.Memory.Slice(0, ProtocolConstants.FrameHeaderSize + written + 1);

            await this.socket.SendAsync(payload);
        }

        public async ValueTask SendContentHeaderAsync(ushort channelNumber, MessageProperties properties, ulong contentSize)
        {
            using var buffer = this.bufferPool.CreateMemory();

            var written = buffer.Memory.Slice(ProtocolConstants.FrameHeaderSize).Span.WriteContentHeader(properties, contentSize);
            buffer.Memory.Span.WriteFrameHeader(FrameType.ContentHeader, channelNumber, (uint)written);
            FrameEndPayload.CopyTo(buffer.Memory.Slice(ProtocolConstants.FrameHeaderSize + written));

            var payload = buffer.Memory.Slice(0, ProtocolConstants.FrameHeaderSize + written + 1);

            await this.socket.SendAsync(payload);
        }

        public async ValueTask SendContentAsync(ushort channelNumber, ReadOnlySequence<byte> contentBytes)
        {
            using var buffer = this.bufferPool.CreateMemory();

            if (contentBytes.IsSingleSegment)
            {
                await this.SendContentChunkAsync(buffer.Memory, channelNumber, contentBytes.First);
            }
            else
            {
                var enumerator = new SequenceEnumerator<byte>(contentBytes);
                while (enumerator.MoveNext())
                {
                    await this.SendContentChunkAsync(buffer.Memory, channelNumber, enumerator.Current);
                }
            }
        }

        private async ValueTask SendContentChunkAsync(Memory<byte> buffer, ushort channelNumber, ReadOnlyMemory<byte> content)
        {
            while (content.Length > 0)
            {
                var chunk = content;
                if (chunk.Length > this.FrameMaxSize)
                {
                    chunk = content.Slice(0, this.FrameMaxSize);
                }

                buffer.Span.WriteFrameHeader(FrameType.ContentBody, channelNumber, (uint)chunk.Length);
                chunk.CopyTo(buffer.Slice(ProtocolConstants.FrameHeaderSize));
                FrameEndPayload.CopyTo(buffer.Slice(ProtocolConstants.FrameHeaderSize + chunk.Length));

                var payload = buffer.Slice(0, ProtocolConstants.FrameHeaderSize + chunk.Length + 1);

                await this.socket.SendAsync(payload);

                content = content.Slice(chunk.Length);
            }
        }

        public void Dispose()
        {
            this.socket.Dispose();
        }
    }
}