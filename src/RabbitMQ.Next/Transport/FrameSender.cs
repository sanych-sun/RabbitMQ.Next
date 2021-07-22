using System;
using System.Buffers;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport
{
    internal class FrameSender : IFrameSender
    {
        private readonly ChannelWriter<MemoryBlock> socketWriter;
        private readonly IMethodRegistry registry;
        private readonly IBufferPool bufferPool;

        public FrameSender(ChannelWriter<MemoryBlock> socketWriter, IMethodRegistry registry, IBufferPool bufferPool)
        {
            if (socketWriter == null)
            {
                throw new ArgumentNullException(nameof(socketWriter));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (bufferPool == null)
            {
                throw new ArgumentNullException(nameof(bufferPool));
            }

            this.socketWriter = socketWriter;
            this.registry = registry;
            this.bufferPool = bufferPool;

            this.FrameMaxSize = ProtocolConstants.FrameMinSize;
        }

        public int FrameMaxSize { get; set; }

        public ValueTask SendMethodAsync<TMethod>(ushort channelNumber, TMethod method)
            where TMethod : struct, IOutgoingMethod
        {
            var buffer = this.bufferPool.CreateMemory();

            var written = this.registry.FormatMessage(method, buffer.Memory.Slice(ProtocolConstants.FrameHeaderSize));
            buffer.Memory.Span.WriteFrameHeader(FrameType.Method, channelNumber, (uint)written);
            buffer.Memory.Span.Slice(ProtocolConstants.FrameHeaderSize + written).Write(ProtocolConstants.FrameEndByte);

            buffer.Slice(ProtocolConstants.FrameHeaderSize + written + 1);

            if (!this.socketWriter.TryWrite(buffer))
            {
                return this.socketWriter.WriteAsync(buffer);
            }

            return default;
        }

        public ValueTask SendContentHeaderAsync(ushort channelNumber, MessageProperties properties, ulong contentSize)
        {
            var buffer = this.bufferPool.CreateMemory();

            var written = buffer.Memory.Slice(ProtocolConstants.FrameHeaderSize).Span.WriteContentHeader(properties, contentSize);
            buffer.Memory.Span.WriteFrameHeader(FrameType.ContentHeader, channelNumber, (uint)written);
            buffer.Memory.Span.Slice(ProtocolConstants.FrameHeaderSize + written).Write(ProtocolConstants.FrameEndByte);

            buffer.Slice(ProtocolConstants.FrameHeaderSize + written + 1);

            if (!this.socketWriter.TryWrite(buffer))
            {
                return this.socketWriter.WriteAsync(buffer);
            }

            return default;
        }

        public async ValueTask SendContentAsync(ushort channelNumber, ReadOnlySequence<byte> contentBytes)
        {
            if (contentBytes.IsSingleSegment)
            {
                await this.SendContentChunkAsync(channelNumber, contentBytes.First);
            }
            else
            {
                var enumerator = new SequenceEnumerator<byte>(contentBytes);
                while (enumerator.MoveNext())
                {
                    await this.SendContentChunkAsync(channelNumber, enumerator.Current);
                }
            }
        }

        private async ValueTask SendContentChunkAsync(ushort channelNumber, ReadOnlyMemory<byte> content)
        {
            while (content.Length > 0)
            {
                var buffer = this.bufferPool.CreateMemory();

                var chunk = content;
                if (chunk.Length > this.FrameMaxSize)
                {
                    chunk = content.Slice(0, this.FrameMaxSize);
                }

                buffer.Memory.Span.WriteFrameHeader(FrameType.ContentBody, channelNumber, (uint)chunk.Length);
                chunk.CopyTo(buffer.Memory.Slice(ProtocolConstants.FrameHeaderSize));
                buffer.Memory.Span.Slice(ProtocolConstants.FrameHeaderSize + chunk.Length).Write(ProtocolConstants.FrameEndByte);

                buffer.Slice(ProtocolConstants.FrameHeaderSize + chunk.Length + 1);

                if (!this.socketWriter.TryWrite(buffer))
                {
                    await this.socketWriter.WriteAsync(buffer);
                }

                content = content.Slice(chunk.Length);
            }
        }
    }
}