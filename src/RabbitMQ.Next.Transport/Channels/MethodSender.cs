using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Channels
{
    internal class MethodSender : IMethodSender
    {
        private readonly ISocketWriter socketWriter;
        private readonly IMethodRegistry registry;
        private readonly ushort channelNumber;
        private readonly SemaphoreSlim writerSync;
        private Memory<byte> writerBuffer;

        public MethodSender(ISocketWriter socketWriter, IMethodRegistry registry, ushort channelNumber)
        {
            this.socketWriter = socketWriter;
            this.registry = registry;
            this.channelNumber = channelNumber;
            this.writerSync = new SemaphoreSlim(1);
            this.writerBuffer = new byte[ProtocolConstants.FrameMinSize];
        }

        public async Task SendAsync<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod
        {
            await this.writerSync.WaitAsync();

            try
            {
                await this.SendMethodFrameInternalAsync(method);
            }
            finally
            {
                this.writerSync.Release();
            }
        }

        public async Task SendAsync<TMethod>(TMethod method, ReadOnlySequence<byte> content)
            where TMethod : struct, IOutgoingMethod
        {
            await this.writerSync.WaitAsync();

            try
            {
                await this.SendMethodFrameInternalAsync(method);
                // TODO: send content frame(s)
            }
            finally
            {
                this.writerSync.Release();
            }
        }

        private Task SendMethodFrameInternalAsync<TMethod>(TMethod method)
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

            var payloadSize = initialSize - payload.Length; // have to add size of methodId
            this.writerBuffer.Span.Slice(0, ProtocolConstants.FrameHeaderSize)
                .WriteFrameHeader(new FrameHeader(FrameType.Method, this.channelNumber, payloadSize));

            this.writerBuffer.Span.Slice(ProtocolConstants.FrameHeaderSize + payloadSize);

            var data = this.writerBuffer.Slice(0, ProtocolConstants.FrameHeaderSize + payloadSize);

            return this.socketWriter.SendAsync(data);
        }
    }
}