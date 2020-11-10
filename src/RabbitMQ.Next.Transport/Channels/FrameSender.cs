using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Buffers;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class FrameSender : IFrameSender
    {
        private readonly ISocketWriter socketWriter;
        private readonly IMethodRegistry registry;
        private readonly ushort channelNumber;
        private readonly IBufferPoolInternal bufferPool;

        public FrameSender(ISocketWriter socketWriter, IMethodRegistry registry, ushort channelNumber, IBufferPoolInternal bufferPool)
        {
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
            memory.Span.WriteFrameHeader(new FrameHeader(FrameType.Method, this.channelNumber, written));

            await this.socketWriter.SendAsync(memory.Slice(0, ProtocolConstants.FrameHeaderSize + written));
        }

        public async Task SendContentHeaderAsync(MessageProperties properties, ulong contentSize)
        {
            using var buffer = this.bufferPool.CreateMemory();
            var memory = buffer.Memory;

            var written = memory.Span.WriteContentHeader(properties, contentSize);
            memory.Span.WriteFrameHeader(new FrameHeader(FrameType.ContentHeader, this.channelNumber, written));

            await this.socketWriter.SendAsync(memory.Slice(0, ProtocolConstants.FrameHeaderSize + written));
        }

        public Task SendContentAsync(ReadOnlySequence<byte> contentBytes)
        {
            return Task.CompletedTask;
        }
    }
}