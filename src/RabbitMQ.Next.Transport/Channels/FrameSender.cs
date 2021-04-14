using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Buffers;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class FrameSender : IFrameSender
    {
        private readonly IConnection connection;
        private readonly ISocketWriter socketWriter;
        private readonly IMethodRegistry registry;
        private readonly ushort channelNumber;
        private readonly IBufferPool bufferPool;

        public FrameSender(IConnection connection, ISocketWriter socketWriter, IMethodRegistry registry, ushort channelNumber, IBufferPool bufferPool)
        {
            this.connection = connection;
            this.socketWriter = socketWriter;
            this.registry = registry;
            this.channelNumber = channelNumber;
            this.bufferPool = bufferPool;
        }

        public async Task SendMethodAsync<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod
        {
            using var buffer = this.bufferPool.CreateMemory();
            var memory = buffer.Memory;

            var written = this.registry.FormatMessage(method, memory.Slice(ProtocolConstants.FrameHeaderSize));
            memory.Span.WriteFrameHeader(FrameType.Method, this.channelNumber, (uint)written);

            var result = memory.Slice(0, ProtocolConstants.FrameHeaderSize + written);
            await this.socketWriter.SendAsync(result);
        }

        public async Task SendContentHeaderAsync(IMessageProperties properties, ulong contentSize)
        {
            using var buffer = this.bufferPool.CreateMemory();
            var memory = buffer.Memory;

            var written = memory.Span.Slice(ProtocolConstants.FrameHeaderSize).WriteContentHeader(properties, contentSize);
            memory.Span.WriteFrameHeader(FrameType.ContentHeader, this.channelNumber, (uint)written);

            await this.socketWriter.SendAsync(memory.Slice(0, ProtocolConstants.FrameHeaderSize + written));
        }

        public Task SendContentAsync(ReadOnlySequence<byte> contentBytes)
        {
            if (contentBytes.IsSingleSegment)
            {
                return this.SendContentSingleChunkAsync(contentBytes.First);
            }

            return this.SendContentMultiChunksAsync(contentBytes);
        }

        private async Task SendContentSingleChunkAsync(ReadOnlyMemory<byte> content)
        {
            using var frameHeaderBuffer = this.bufferPool.CreateMemory(ProtocolConstants.FrameHeaderSize);

            while (content.Length > 0)
            {
                var chunk = content;
                if (chunk.Length > this.connection.Details.FrameMaxSize)
                {
                    chunk = content.Slice(0, this.connection.Details.FrameMaxSize);
                }

                await this.SendContentFrameAsync(frameHeaderBuffer.Memory, chunk);

                content = content.Slice(chunk.Length);
            }

        }

        private async Task SendContentMultiChunksAsync(ReadOnlySequence<byte> contentBytes)
        {
            using var frameHeaderBuffer = this.bufferPool.CreateMemory(ProtocolConstants.FrameHeaderSize);
            using var contentFrameBuffer = this.bufferPool.CreateMemory();

            var enumerator = new SequenceEnumerator<byte>(contentBytes);
            ReadOnlyMemory<byte> pending = default;
            while (enumerator.MoveNext())
            {
                if (pending.Length == 0)
                {
                    pending = enumerator.Current;
                }
                else if (pending.Length + enumerator.Current.Length > this.connection.Details.FrameMaxSize)
                {
                    await this.SendContentFrameAsync(frameHeaderBuffer.Memory, pending);
                    pending = enumerator.Current;
                }
                else
                {
                    pending.CopyTo(contentFrameBuffer.Memory);
                    var rest = contentFrameBuffer.Memory.Slice(pending.Length);
                    enumerator.Current.CopyTo(rest);

                    pending = contentFrameBuffer.Memory.Slice(0, pending.Length + enumerator.Current.Length);
                }

                while (pending.Length > this.connection.Details.FrameMaxSize)
                {
                    var chunk = pending.Slice(0, this.connection.Details.FrameMaxSize);
                    await this.SendContentFrameAsync(frameHeaderBuffer.Memory, chunk);
                    pending = pending.Slice(chunk.Length);
                }
            }


            if (pending.Length > 0)
            {
                await this.SendContentFrameAsync(frameHeaderBuffer.Memory, pending);
            }
        }

        private async Task SendContentFrameAsync(Memory<byte> headerBuffer, ReadOnlyMemory<byte> chunk)
        {
            headerBuffer.Span.WriteFrameHeader(FrameType.ContentBody, this.channelNumber, (uint)chunk.Length);

            await this.socketWriter.SendAsync(( headerBuffer, chunk), async (sender, state) =>
            {
                await sender(state.headerBuffer);
                await sender(state.chunk);
            });
        }
    }
}