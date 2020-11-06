using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class
        FrameSender : IFrameSender
    {
        private readonly ISocketWriter socketWriter;
        private readonly IMethodRegistry registry;
        private readonly ushort channelNumber;
        // TODO: use array pool instead
        private readonly Memory<byte> writerBuffer;
        private readonly int maxFrameSize;

        public FrameSender(ISocketWriter socketWriter, IMethodRegistry registry, ushort channelNumber, int maxFrameSize = ProtocolConstants.FrameMinSize)
        {
            this.socketWriter = socketWriter;
            this.registry = registry;
            this.channelNumber = channelNumber;
            this.writerBuffer = new byte[maxFrameSize + ProtocolConstants.FrameHeaderSize];
            this.maxFrameSize = maxFrameSize;
        }

        public Task SendMethodAsync<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod
        {
            var formatter = this.registry.GetFormatter<TMethod>();
            if (formatter == null)
            {
                // TODO: throw connection-level exception here?
                throw new InvalidOperationException();
            }

            // TODO: implement functionality to extend buffer if connection configured to use bigger frames
            // and data does not fit into default framesize
            var payload = this.writerBuffer.Span.Slice(ProtocolConstants.FrameHeaderSize);
            var initialSize = payload.Length;
            payload = payload.Write(method.Method);
            payload = formatter.Write(payload, method);

            var payloadSize = initialSize - payload.Length;
            this.writerBuffer.Span.Slice(0, ProtocolConstants.FrameHeaderSize)
                .WriteFrameHeader(new FrameHeader(FrameType.Method, this.channelNumber, payloadSize));

            var data = this.writerBuffer.Slice(0, ProtocolConstants.FrameHeaderSize + payloadSize);

            return this.socketWriter.SendAsync(data);
        }

        public Task SendContentHeaderAsync(MessageProperties properties, ulong contentSize)
        {
            // TODO: implement functionality to extend buffer if connection configured to use bigger frames
            // and data does not fit into default framesize
            var payload = this.writerBuffer.Span.Slice(ProtocolConstants.FrameHeaderSize);
            var initialSize = payload.Length;
            payload = payload
                .Write((ushort) ClassId.Basic)
                .Write((ushort) ProtocolConstants.ObsoleteField)
                .Write(contentSize)
                .WriteMessageProperties(properties);

            var payloadSize = initialSize - payload.Length;
            this.writerBuffer.Span.Slice(0, ProtocolConstants.FrameHeaderSize)
                .WriteFrameHeader(new FrameHeader(FrameType.ContentHeader, this.channelNumber, payloadSize));

            var data = this.writerBuffer.Slice(0, ProtocolConstants.FrameHeaderSize + payloadSize);

            return this.socketWriter.SendAsync(data);
        }

        // TODO: implement some magic here to make serializers preformat ReadOnlySequence according to maxframesize to avoid CopyTo
        public async Task SendContentAsync(ReadOnlySequence<byte> contentBytes)
        {
            while (contentBytes.Length > 0)
            {
                var data = this.PrepareChunk(contentBytes, out int chunkSize);
                await this.socketWriter.SendAsync(data);

                contentBytes = contentBytes.Slice(chunkSize);
            }
        }

        private Memory<byte> PrepareChunk(ReadOnlySequence<byte> contentBytes, out int chunkSize)
        {
            chunkSize = (int)Math.Min(contentBytes.Length, this.maxFrameSize);

            var payload = this.writerBuffer.Span.WriteFrameHeader(new FrameHeader(FrameType.ContentBody, this.channelNumber, chunkSize));

            contentBytes.Slice(0, chunkSize).CopyTo(payload);

            return this.writerBuffer.Slice(0, ProtocolConstants.FrameHeaderSize + chunkSize);
        }
    }
}